# Commands and sessions

[README](../README.md) · [Getting started](getting-started.md) · [Fluent](fluent.md)

## Choose a command type

| Type | Use it when |
| --- | --- |
| `IQueryCommand<TQuery>` | An incoming command or callback can be handled independently |
| `ISessionCommand<TQuery, TData>` | The next update needs data saved by an earlier step |
| `IBehaviorCommand<TData>` | An active conversation should inspect or intercept other commands |
| `FluentCommand<TData, TState>` | The conversation is naturally expressed as named states and transitions |

These are the primary messenger-independent contracts, not aliases over legacy
Telegram interfaces. Results, descriptors, factories, and exceptions also use neutral
names. The old Telegram-prefixed contracts are not retained in the shared projects.

All types live under `Bot.Commands.Abstract.Commands`, except `FluentCommand`,
which lives under `Bot.Commands.Core.Fluent`.

## Query context

Query and session command handlers receive `IQueryContext<TQuery>`:

```csharp
Task<ICommandExecutionResult> Execute(IQueryContext<TQuery> query);
Task<ICommandExecutionResult> Execute(IQueryContext<TQuery> query, TData data);
```

These are signatures, not standalone implementations. The context provides:

| Member | Meaning |
| --- | --- |
| `Query` | Original SDK query, strongly typed |
| `GetChatId()` | Destination/session chat ID |
| `GetFromId()` | User who initiated the update |
| `GetData()` | Message text, callback payload, or other adapter-specific data; may be null |
| `GetMessageId()` | String message identifier, or null for queries without a message |
| `GetChatType()` | Common `ChatArea` value |
| `IsGroupMessage()` | Whether the context is a group message |
| `IsEvent()` / `GetEventType()` | Common event classification |

IDs retain platform semantics. Do not assume a Max message ID can be parsed as an integer.
See [adapter mappings](adapters.md) for available query types and exceptions.

## Metadata and discovery

```csharp
[Command(
    Name = "report",
    Authorized = true,
    Area = ChatArea.Private | ChatArea.Group | ChatArea.SuperGroup,
    Swarms = new[] { "staff" })]
```

The attribute belongs on a concrete command class. `Name` excludes the leading
slash. Defaults are `Authorized = true` and `Area = ChatArea.Private`.

Commands are discovered from loaded assemblies. Selection uses the command name,
the incoming SDK query type, and the bot's swarm configuration. Separate Telegram
and Max command classes may share a name. One class may also implement both typed
interfaces; see [the shared example](examples/QuickStart/Commands.cs).

Command names are matched case-sensitively. Telegram bot mentions are checked
case-insensitively. Duplicate compatible candidates are an error; a matching swarm
does not establish priority between multiple remaining candidates.

### Command arguments

`/report@my_bot weekly` selects `report`. In a command handler, use the extension
from `Bot.Commands.Core` to obtain arguments:

```csharp
var arguments = this.ExtractData(context); // "weekly"
```

This removes the first whitespace-delimited command token and trims the remainder.
It is not a shell-style argument parser: quoting, named options, and validation
belong in the application. `context.GetData()` returns the full original payload.

### Swarms

When `IBotProfile.Swarms` is nonempty, candidates remain eligible if their
`CommandAttribute.Swarms` is null or intersects the profile's groups. Commands
with null swarms are therefore shared. An empty command swarm array does not
mean shared. When the profile has no swarms, no swarm filtering is applied.

Swarms select command implementations; they are not a substitute for user authorization.

## Execution results

Return `ICommandExecutionResult`, normally created through
`CommandExecutionResult` in `Bot.Commands.Core.Models`.

| Factory | Effect after successful execution |
| --- | --- |
| `Break()` | Release the active session, if any |
| `Freeze()` | Leave session metadata unchanged; no open, continuation, or release |
| `Ahead<TNext, TQuery, TData>(data, duration)` | Open or continue a typed session |
| `Ahead<TBehavior, TData>(data, duration)` | Open or continue a behavior session |
| `AheadFluent<TFluent, TData, TState>(data, fire: false, sessionDurationInSec: ...)` | Store an uninitialized Fluent session for the next update |

`Ahead` chooses what handles a later update; it does not immediately execute the
next command. Overloads also support `EmptyObject` and moving the session to another
chat via `moveToChatId`. The user ID is retained when changing chats.

The enum also contains `ExecuteResult.Fire`, and `AheadFluent(..., fire: true)` can
produce it. **The current resolver does not handle this result.** Use direct entry
into a Fluent command or `fire: false`; do not use `Fire` in an integration yet.

`Freeze()` is not a transaction rollback. Changes already made to an in-memory
session object or external system are not undone.

## A two-step interaction

The [runnable sample](examples/QuickStart/Commands.cs) implements this flow:

1. `/ask` selects `AskCommand`, which sends a prompt.
2. The command returns `Ahead<NameReply, Message, NameData>(data)`.
3. The next message is delivered to `NameReply` with the saved data.
4. `NameReply` returns `Break()` after accepting the answer, or `Freeze()` to keep waiting.

A session-only command is entered through a session transition; its attribute name
identifies the persisted target. It is not automatically a standalone slash command.
Implement `IQueryCommand<TQuery>` too if you intentionally provide a separate entry path.

An ordinary active session takes precedence over newly typed slash commands.
For example, `/cancel` can be delivered as input to a session handler instead of
executing a global cancel command. Use a behavior or Fluent interception if commands
must remain available during a conversation.

## Behavior commands

`IBehaviorCommand<TData>` exposes these inherited methods:

```csharp
Task<ICommandExecutionResult> DefaultExecute<TQuery>(
    IQueryContext<TQuery> query, TData data);

Task<ICommandExecutionResult> Execute<TQuery>(
    IQueryCommand<TQuery> currentCommand,
    IQueryContext<TQuery> query, TData data);

Task<ICommandExecutionResult> Execute<TQuery>(
    ISessionCommand<TQuery, TData> currentCommand,
    IQueryContext<TQuery> query, TData data);
```

When a behavior is active, the engine resolves a command from the incoming query
when possible and passes it to the appropriate overload. Otherwise it calls
`DefaultExecute`. The behavior decides whether to execute, reject, or ignore that
command. For the session overload, the intercepted command's data type must match
the behavior's data type.

A directly invoked behavior receives null initial data. Initialize it in
`DefaultExecute` or enter the behavior through `Ahead` with explicit data.
See [GetPhotoMenuBehavior](../Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers/Services/Commands/Photos/GetPhotoMenuBehavior.cs)
for a concrete example.

## Authorization and chat areas

For attributes with `Authorized = true`, the engine calls
`IAuthProvider.AuthUser<TQuery>(userId, executionContext)` before execution.
That context includes the native query, command type, metadata, and chat ID.
Returning false raises `CommandPermissionException`.

The engine also checks `ChatArea` flags and raises `CommandChatAreaException`
for a disallowed area. Telegram groups and supergroups have separate flags.
For behavior composition, the relevant session and query command descriptors are checked.

Authorization failures and domain exceptions are not automatically sent to users.
Handle them at your update boundary and decide which messages to expose.

## Execution filters

Derive a command attribute from `ExecuteFilterAttribute` to add an asynchronous
pre-execution check shared by all adapters:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AllowedChatAttribute(long allowedChatId) : ExecuteFilterAttribute
{
    public override Task Filter<TQuery>(ICommandExecutionContext<TQuery> context)
    {
        if (context.ChatId != allowedChatId)
            throw new CommandPermissionException("This chat is not allowed", context.ChatId);
        return Task.CompletedTask;
    }
}

// Apply alongside CommandAttribute on a concrete or shared abstract command:
// [AllowedChat(12345, Order = 10)]
```

The common resolver checks authorization and chat area before awaiting filters.
It then executes the command and applies its session result. A filter exception
propagates to the update boundary: later filters, command execution and session
updates are skipped. Filter side effects themselves are not rolled back.

Inherited filters are included according to their `AttributeUsage`. Declare
`AllowMultiple = true` on a derived filter when repeated instances should coexist,
including on a base class. Lower `Order` values run first within each command;
use distinct values when ordering matters.

For behavior composition, the active session command's filters run first, followed
by those of the selected query command, before either handler executes. This is a
check of the resolved composition, even if the behavior later intercepts that
query command. Each distinct command type is filtered once per update.
The execution context identifies that concrete command type and descriptor and
provides its native query and chat ID. Attribute instances are created for the
invocation; they are not resolved from dependency injection.

## Events

Use an `EventAttribute` and a compatible `IQueryCommand<TQuery>` for events exposed
by an adapter. For example, this attribute marks a Telegram message handler for
new chat members:

```csharp
[Event(Type = EventType.ChatMemberAdded, Authorized = false,
       Area = ChatArea.Group | ChatArea.SuperGroup)]
```

The handler's query type is `Telegram.Bot.Types.Message` for Telegram new-member
messages, and `Max.Bot.Types.UserAddedUpdate` for Max user-added updates.
Event resolution is separate from ordinary session command resolution. An event
with no compatible handler is ignored. Only one compatible event handler is selected;
`EventAttribute.Order` does not currently implement ordered fan-out. Event lookup
does not apply ordinary command swarm filtering.

The event enum lists more events than the adapters currently emit. Refer to the
[supported-update table](adapters.md#supported-updates), not the enum alone.

## Session storage contract

`ISessionsStore` is application-provided. It must support:

| Method | Required behavior |
| --- | --- |
| `GetSessionInfo(now, chatId, userId)` | Return the active session's metadata, or null |
| `GetSessionInfoWithData(..., commandQuery, Type)` | Return matching data deserialized to the requested type |
| `GetSessionInfoWithData<T>(...)` | Return the typed equivalent for session helper APIs |
| `CreateSession(session)` | Persist command, IDs, timestamps, and data |
| `UpdateSession(session, chatIdFrom)` | Update the old chat/user entry, including a changed chat ID |

Exclude sessions with `ExpiredAt <= now`. A null expiry means no expiration.
Release is represented by an update setting `ExpiredAt` to the clock's current
time and clearing data; there is no separate delete method in the contract.

The default `Ahead` duration is 600 seconds. On continuation, a finite duration
is added to the existing expiry, or to `Now` if the previous expiry was null.
It is **not** always reset to `Now + duration`. Passing null removes expiration.
Fluent states have their own nullable durations; set them explicitly when needed.

Session contracts use neutral `ChatId` and `UserId` property names.
Existing stores using `TelegramChatId`/`TelegramUserId` require a schema/serialization
migration or an explicit mapping in the store implementation. Neither platform nor bot
identity is part of the store key. Use separate storage partitions or explicitly
bot-aware store instances. Scope creation alone does not provide that separation.

Store application data, not SDK clients or DI services. A persistent Fluent session
must preserve the `FluentObject<TData, TState>` wrapper, including the current state
and initialization flag. Plan migrations when renaming commands or changing stored
data/state types. The [demo store](examples/QuickStart/Infrastructure.cs) is sequential
and in-memory; it is intended to explain the contract rather than supply production storage.
