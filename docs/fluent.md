# Fluent conversations

[README](../README.md) · [Commands](commands.md) · [Complete example](examples/QuickStart/Commands.cs)

`FluentCommand<TData, TState>` is a behavior command backed by a state machine.
It separates the conversation definition from data carried between updates.
Use an enum for `TState` and a small application-owned class for `TData`.

## Define the entry point and states

A subclass supplies two methods:

```csharp
protected override Task<(ProfileData, ProfileStep)> Entry<TQuery>(
    IQueryContext<TQuery> query, ProfileData currentObject)
    => Task.FromResult((currentObject ?? new ProfileData(), ProfileStep.Name));

protected override IStateMachine<ProfileStep> StateMachine(
    IStateMachineBuilder<ProfileData, ProfileStep> builder)
{
    return builder
        .State(ProfileStep.Name)
        .WithMessage("What is your name?", Send)
        .Next<object>((query, state, data) =>
        {
            data.Name = ReadText(query);
            return Task.FromResult(
                string.IsNullOrWhiteSpace(data.Name) ? ProfileStep.Name : ProfileStep.Done);
        }, force: true)
        .Exit<object>(ProfileStep.Done, Finish)
        .Build();
}
```

This excerpt comes from [ProfileCommand](examples/QuickStart/Commands.cs), which also
defines `Send`, `ReadText`, `Finish`, the data class, and state enum.

When `/profile` is received, `Entry` returns initial data and the first state.
The engine sends that state's prompt and persists a Fluent session. The next
update reaches the state's handler. Returning the exit state runs `Finish`, whose
execution result determines whether the session ends or continues elsewhere.

`Entry` and `GlobalIntercept` receive `IQueryContext<TQuery>`. Read common metadata
through `GetChatId()` and `GetData()`; the original SDK object is available as
`Query`. Fluent handlers receive that same context through `IQueryContext<TQuery>`.

## Handler types: contexts versus SDK objects

The distinction below is part of the current API:

| Callback | Receives |
| --- | --- |
| Command `Execute` / `DefaultExecute` | `IQueryContext<TQuery>` |
| Typed `WithMessage<TQuery>` sender | `IQueryContext<TQuery>` |
| Shared `WithMessage` sender | `IQueryContext<object>` |
| State-level batch sender | `IQueryContext<object>` |
| Fluent `Entry`, `GlobalIntercept` | `IQueryContext<TQuery>` |
| `Next<TQuery>`, `OnCallback<TQuery>`, `Exit<TQuery>` | `IQueryContext<TQuery>` |

A `Next<Telegram.Bot.Types.Message>` handler expects a context containing a
Telegram message. It does not adapt Max messages or Telegram callbacks.
The covariant generic interface lets shared handlers accept `IQueryContext<object>`
without creating wrappers or losing context metadata. There is no non-generic
`IQueryContext` interface.

For a shared flow with distinct message and callback handlers, make the command
generic: `MyFluentCommandFluent<TMessage, TCallback>`, with both parameters
constrained to `class`. Use `Next<TMessage>` and `OnCallback<TCallback>` inside it.
Concrete Telegram and Max subclasses supply their SDK types. Senders and exit
handlers that accept either update kind can use `IQueryContext<object>`.

## State operations

| Operation | Behavior |
| --- | --- |
| `State(id, durationInSec: ...)` | Define a waiting state and its session duration |
| `WithMessage(...)` | Add one outgoing message and its sender |
| `WithMessages(...)` | Build messages dynamically from state and data |
| `Next(handler, force)` | Choose the next state from incoming input |
| `Next(stateId, force)` | Always choose a fixed next state |
| `Loop(force)` | Choose the current state again |
| `WithCallbacks().Row()` | Start a row of buttons for the current message |
| `OnCallback(...)` | Run a button handler that chooses the next state |
| `NextFromCallback(...)` | Choose a fixed state from a button |
| `ExitFromCallback<TCommand>(...)` | Associate a button with an external command by its attributed name |
| `Exit(id, finalizer)` | Define a terminal state whose handler returns a command result |
| `FireAndForget()` | Send the state's messages and finish without waiting for input |

When the next state differs from the current state, its messages are sent.
`force: true` also sends them when remaining in the same state. This is useful for
validation prompts and refreshing menus; it does not execute an arbitrary chain
of state handlers immediately.

`FireAndForget()` is a state-builder operation and works independently of the
unsupported `ExecuteResult.Fire` result. Do not confuse it with
`AheadFluent(..., fire: true)`.

## Send messages

To leave a flow through a command button, specify the target type directly:

```csharp
.WithCallbacks().Row()
.ExitFromCallback<CancelCallback>("Cancel", "someData")
```

The overloads also accept `CallbackData` or a `(state, data) => CallbackData`
provider. They are available for regular rows, generated keyboards, and dynamic
message rows. The target must have `CommandAttribute`; an attributed abstract
base is valid. Only its command descriptor is captured, so the current platform
and any configured swarms select the concrete implementation. Existing descriptor-based
overloads remain available.

Message providers return `ITextMessage`; senders receive the constructed `IBotMessage`
including callbacks. You can supply a sender per message or one batch sender on
the state. When a batch sender is configured, the state gathers messages and uses
it instead of individual senders. Configure at least one applicable sender for
states that produce messages.

For a Telegram-only typed sender:

```csharp
// Fragment inside a class with an injected ITelegramBotClient named client.
Task Send(IQueryContext<Telegram.Bot.Types.Message> context,
    ProfileStep state, ProfileData data, IBotMessage message)
    => client.SendMessage(context.GetChatId(), message.Text,
        parseMode: message.Format.ToParseMode(),
        replyMarkup: message.ToReplyMarkup());
```

Import `Bot.Commands.Telegram.Extensions` for conversion helpers and
`Telegram.Bot` for SDK extension methods. This sender only accepts Telegram message
contexts. A state reached from a callback or Max update needs a suitable sender
or an untyped sender that selects the correct platform implementation.

The framework does not automatically choose between sending a new message and
editing an existing one. Use `GetMessageId()` and the SDK to implement your desired
behavior. Photo upload and callback acknowledgment also belong to your sender/host.

## Buttons and callback data

The callback builder creates internal `fid` and `fud` fields to route a button
back to its state and user data. Preserve generated payloads when converting a
keyboard to SDK types. Do not replace them with labels or arbitrary IDs.

Keep user data short and simple. The current internal encoding concatenates
`&` and `=` delimiters without escaping; arbitrary strings containing those
characters are not safe round-trip values. Prefer compact application identifiers.
Platform payload-size limits still apply. Shared keyboard construction does not
guarantee that every platform-specific size limit has been validated.

Dynamic providers may run again while handling a callback. Generate stable message
and row ordering so that callback IDs continue to identify the intended button.
The current implementation is not a persistent registry of independently versioned menus.

## Interception

Override `GlobalIntercept<TQuery>` for logic that runs before normal state handling.
Return `GlobalInterceptResult.Next` to continue. Returning `Freeze` keeps the current
Fluent state, but the Fluent implementation returns an `AheadFluent` continuation;
session persistence/expiry behavior therefore differs from a raw
`CommandExecutionResult.Freeze()`.

For selective command interception, override `TryQueryCommandIntercept` and return
`InterceptResult<TQuery, TData>`:

- `MustIntercept` executes the selected query command.
- `MustInterceptWithoutExecute` intercepts without calling it.
- `IsTerminalResult` propagates the executed command's result, allowing it to end or replace the session.
- `ResultCallback` runs after a nonterminal executed command.

See [FluentCommand](../Telegram.Commands/src/Bot.Commands.Core/Fluent/FluentCommand.cs)
for the exact hook signatures and [the global-interception sample](../Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers/Services/Commands/GlobalIntercept/FluentCommandWithGlobalIntercept.cs)
for a concrete implementation.

## Persistence and lifetime

The session stores a `FluentObject<TData, TState>`, not only `TData`. It includes
the current state, initialization status, and entry mode. Preserve the wrapper
when serializing and restoring sessions.

State definitions and message builders contain mutable runtime state. Use the
provided scoped command registrations and process updates for each session in
order. Do not share one command instance across concurrent updates.

For timeouts, specify `durationInSec` on the state. A default null state duration
means no expiration when its continuation is stored; it does not inherit the
600-second default of the ordinary `Ahead` factory.
