# Bot.Commands

**Commands, sessions, and fluent conversation flows for Telegram and Max bots in .NET.**

Bot.Commands separates conversation logic from incoming messenger updates. Use typed commands for individual actions, session commands for multi-step interactions, and `FluentCommand` to describe a conversation as a state machine.

The shared engine uses messenger-independent `Bot.Commands.*` contracts. Adapters for **Telegram** and **Max** own SDK integration, command addressing, and formatting rules. This is a breaking source/API migration from the earlier Telegram-prefixed contracts; see the [migration guide](docs/migration.md). The repository's historical `Telegram.Commands/src` directory and solution filename are unchanged.

## What you can build

- Route `/command arguments` messages and callback buttons to typed handlers.
- Carry application data between steps using your own session store.
- Describe prompts, validation, transitions, and exit actions with a fluent API.
- Intercept commands inside an active conversation using behavior commands.
- Restrict commands by authorization, chat type, and bot-specific command groups.
- Build common inline keyboards and convert them to Telegram or Max SDK types.

The library handles command execution and conversation state. Your application owns update delivery, SDK clients, outgoing messages, storage, and hosting.

## A conversation in code

```csharp
// Inside a FluentCommand<ProfileData, ProfileStep> subclass.
return builder
    .State(ProfileStep.Name)
    .WithMessage("What is your name?", Send)
    .Next<object>((query, state, data) =>
    {
        data.Name = ReadText(query);
        return Task.FromResult(ProfileStep.Done);
    }, force: false)
    .Exit<object>(ProfileStep.Done, Finish)
    .Build();
```

`Send`, `ReadText`, and `Finish` are application methods. See the [complete, runnable example](docs/examples/QuickStart/Commands.cs) and the [Fluent guide](docs/fluent.md) for their implementations and handler contracts.

## Try it locally

Requires the **.NET 10 SDK**. Run these commands from the repository root:

```sh
dotnet restore Telegram.Commands/src/Telegram.Commands.sln
dotnet build Telegram.Commands/src/Telegram.Commands.sln --no-restore
dotnet run --project docs/examples/QuickStart -- telegram
dotnet run --project docs/examples/QuickStart -- max
```

The quick-start example processes synthetic SDK updates and writes replies to the console. It requires no bot token, makes no bot API calls, and demonstrates the same commands and Fluent flow through either adapter.

For a real bot, start with [installation and integration](docs/getting-started.md). The guide uses project references to this checkout; it does not assume that a matching NuGet release has already been published.

## Documentation

| Guide | Contents |
| --- | --- |
| [Getting started](docs/getting-started.md) | Requirements, registration, SDK integration, update scopes |
| [Commands and sessions](docs/commands.md) | Command types, routing, results, authorization, events, storage contract |
| [Fluent conversations](docs/fluent.md) | States, messages, callbacks, transitions, interception |
| [Messenger adapters](docs/adapters.md) | Supported updates, keyboard conversion, platform differences |
| [Architecture and operations](docs/architecture.md) | Project boundaries, execution flow, concurrency, troubleshooting |
| [Migration](docs/migration.md) | Moving from the older Telegram-only API |
| [Contributing](CONTRIBUTING.md) | Build, tests, documentation changes, release preparation |

## Project layout

| Project | Responsibility |
| --- | --- |
| `Bot.Commands.Abstract` | Command contracts, query contexts, metadata, common messages |
| `Bot.Commands.Core` | Resolution, execution, sessions, Fluent state machines |
| `Bot.Commands.UI` | Dropdown and pagination helpers |
| `Bot.Commands.Telegram` | Telegram update contexts and SDK conversions |
| `Bot.Commands.Max` | Max update contexts and SDK conversions |
| `Bot.Commands.DependencyInjection` | Microsoft DI registration and command factory |

Abstract, Core, UI, and common DependencyInjection do not reference a messenger SDK or adapter. Each adapter references the common registration project and provides its own registration extension. See [architecture](docs/architecture.md) for dependency details.

## Current scope

The code targets .NET 10 and currently references Telegram.Bot **22.9.0** and MaxMessenger.Bot **0.4.1-alpha**. These are the versions in this repository, not a claim about the latest upstream releases.

Not every update or SDK feature is exposed by the adapters. Sessions are keyed by chat and user, so applications running multiple bots must isolate their stores and profiles. The engine does not provide update deduplication, per-session locking, or automatic retries. Read the [operational notes](docs/architecture.md#operational-boundaries) before connecting a persistent store or multiple workers.

## Build and test

```sh
dotnet build Telegram.Commands/src/Telegram.Commands.sln
dotnet test Telegram.Commands/src/Telegram.Command.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests.csproj
```

Tests cover command results, messenger-specific routing, shared session commands, Fluent query handling, and keyboard rows. They do not replace live SDK integration tests.

## License status

The project metadata references MIT, but this checkout does not include a standalone `LICENSE` file. The repository owner should confirm the intended license and add its full text before publishing a release. This README does not grant a license itself.
