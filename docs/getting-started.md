# Getting started

[README](../README.md) · [Commands](commands.md) · [Fluent](fluent.md) · [Adapters](adapters.md)

## Requirements

- .NET 10 SDK.
- A checkout of this repository.
- For a live integration: a bot account and credentials for the chosen messenger,
  plus an application that receives its updates.

The repository pins its SDK dependencies in the adapter `.csproj` files. The
instructions below build the current source; they are not instructions for an
older NuGet release with a similar package name.

## 1. Run the offline example

From the repository root:

```sh
dotnet run --project docs/examples/QuickStart -- telegram
dotnet run --project docs/examples/QuickStart -- max
```

Both commands should produce the same bot replies:

```text
bot: Hello! Try /ask or /profile.
bot: What should I call you?
bot: Nice to meet you, Ada.
bot: What is your name?
bot: Profile saved for Grace.
```

Additional lines show the platform and simulated user input. Each update is processed
inside a new DI scope. The in-memory store survives those scopes but is discarded
when the program exits. The example deliberately uses console output instead of
calling a messenger API.

Read the example in this order:

1. [Program.cs](examples/QuickStart/Program.cs): registration and update dispatch.
2. [Commands.cs](examples/QuickStart/Commands.cs): simple commands, a session, and a Fluent flow.
3. [Infrastructure.cs](examples/QuickStart/Infrastructure.cs): profile, authorization, clock, and demo storage.

## 2. Reference the library from your application

For an application at `MyBot/MyBot.csproj`, with this checkout available as
`telegram/`, run from their parent directory:

```sh
dotnet add MyBot/MyBot.csproj reference telegram/Telegram.Commands/src/Bot.Commands.Telegram/Bot.Commands.Telegram.csproj
dotnet add MyBot/MyBot.csproj package Microsoft.Extensions.DependencyInjection --version 10.0.3
```

This references the Telegram adapter, which brings in the shared Core, Abstract,
and DependencyInjection projects. For Max, reference
`telegram/Telegram.Commands/src/Bot.Commands.Max/Bot.Commands.Max.csproj` instead.
Reference `Bot.Commands.UI` separately if you need dropdown/pagination helpers.
An ASP.NET Core application already has the DI container through its framework.
Referencing the common DI project alone does not install either adapter or SDK;
see [architecture](architecture.md).

## 3. Register application services

The engine needs four application-owned services:

| Contract | Responsibility | Typical lifetime |
| --- | --- | --- |
| `IBotProfile` | Bot name, credentials, command groups, host metadata | Singleton for one immutable profile |
| `IAuthProvider` | Decide whether a user may execute a protected command | Scoped if it uses a database |
| `IClock` | Supply a consistent `DateTime` for session expiry | Singleton |
| `ISessionsStore` | Load and persist command state | Scoped database facade or shared durable service |

The following is a registration fragment. The named implementations are supplied
by your application; the offline example includes demo equivalents.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.DependencyInjection;

services.AddSingleton<IBotProfile>(botProfile);
services.AddSingleton<IClock, UtcClock>();
services.AddScoped<IAuthProvider, ApplicationAuthorization>();
services.AddScoped<ISessionsStore, DatabaseSessionStore>();
services.UseTelegramCommandsServices();
```

For Max, replace the final call with `services.UseMaxCommandsServices()`.

`UseTelegramCommandsServices()` registers `ITelegramBotClient` using
`IBotProfile.Key`, unless a client has already been registered. `UseMaxCommandsServices()`
does not register a Max client. Register the client or sender service required by
your Max command constructors explicitly.

Do not use a fresh, empty in-memory store per update: the next update would lose
the previous step. Do not register command instances or `FluentCommand` instances
as singletons; the provided registrations use scoped lifetimes.

## 4. Write a command

This is a complete Telegram command. It uses `Authorized = false` so that the
greeting is public; authorization is enabled by default on command attributes.

```csharp
using Telegram.Bot;
using Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;

[Command(Name = "hello", Authorized = false)]
public sealed class HelloCommand(ITelegramBotClient client) : IQueryCommand<Message>
{
    public async Task<ICommandExecutionResult> Execute(IQueryContext<Message> context)
    {
        await client.SendMessage(context.GetChatId(), "Hello!");
        return CommandExecutionResult.Break();
    }
}
```

For Max, implement `IQueryCommand<Max.Bot.Types.Message>` and inject your Max sender.
The common context exposes chat ID, sender ID, text/payload, chat area, and message
ID. Access the SDK object through `context.Query` when you need platform-specific data.

Command discovery scans currently loaded assemblies that depend directly or transitively
on `Bot.Commands.Abstract` for concrete attributed classes. Unrelated SDK and database
assemblies are not inspected; loader errors in command assemblies remain visible.
Load assemblies containing commands before calling the registration extension.
Constructor dependencies must also be registered. The built-in factory resolves
commands by their concrete type.

## 5. Forward updates to the resolver

In a polling worker, create a scope for each update:

```csharp
// Fragment inside your update receiver; update is a Telegram.Bot.Types.Update.
using var scope = serviceProvider.CreateScope();
var resolver = scope.ServiceProvider
    .GetRequiredService<IBotCommandResolver<Telegram.Bot.Types.Update>>();
await resolver.Handle(update);
```

For Max, use `IBotCommandResolver<Max.Bot.Types.Update>`. In an ASP.NET Core request,
the request already has a scope, so inject the appropriate resolver into the
controller or endpoint handler.

The library does not start polling, register webhooks, validate webhook requests,
or acknowledge callback/payment queries automatically. Keep those responsibilities
in the host and SDK integration. Do not call `Handle` concurrently for the same
session unless the host supplies serialization or locking.

## 6. Replace demo infrastructure before deployment

Implement the [session storage contract](commands.md#session-storage-contract),
provide an authorization policy, and configure SDK credentials outside source code.
Isolate profiles and sessions by bot/platform when hosting multiple bots. A separate
DI scope alone does not isolate singleton storage or select a different bot profile.

Continue with [commands and sessions](commands.md) or [Fluent conversations](fluent.md).
