# Nested dialogue example

Start `/routeorder` on any enabled SimpleHandlers bot. The scenario is:

1. Enter a name in `OrderFlow`.
2. Enter a street in the child `AddressFlow`.
3. Enter a city in its child `CityFlow`.
4. Save the address to resume the order review, then finish the order.

Blank answers stay on the input step. Cancelling a city returns to the address;
cancelling the address returns to the order without replacing its previous
address. The review buttons can open a new child flow. Saving an address without
a city opens the city flow again. Cancelling the order completes the session.

The three generic flows contain the business logic. Telegram, Max, VK,
Instagram and WhatsApp descendants supply the native message type and swarm.
There is no static route map. Each `Call` creates a new route ID and captures
the current caller, its data and a runtime return state. Each parent processes
the result in its own `OnReturn` method, with normal DI dependencies. Caller
snapshots and return states live in one `Routes` collection in normal session data,
linked by `ParentRouteId`. Snapshots exclude that collection. Returned/cancelled
routes retain their outcome until the session ends. No `IRouteStore` or session contract
changes are needed. The host continues to isolate sessions by BotName.

Routing exits use `.Route(state, (q, data) => Call<...>(q, data, returnState, childData))`,
`.ReturnRoute(state, data => result)` and `.CancelRoute(state)`.
Import `Bot.Commands.Core.Fluent.Routing`. `Route` also supports conditional
call/return expressions without `Exit<object>` or `Task.FromResult` wrappers.

## In-memory storage

Set `Sessions:UseInMemory` to `true` in local configuration, or set the environment
variable `Sessions__UseInMemory=true` before starting SimpleHandlers. This selects
`UseInMemoryStorage()` and skips the demo database reset at startup. The default
remains the existing EntityStorage implementation. In-memory state disappears
when the process stops.

Storage selection alone does **not** enable delivery deduplication in the HTTP
controllers. The optional in-memory update boundary is demonstrated by
`InMemoryRoutingBehaviorTests`, which runs these exact commands through Telegram
and Max resolvers in independent DI containers sharing one memory backend.
It uses barriers to overlap updates, rather than sleeps or mocks of Fluent.

## Optional process-local update boundary

The following host fragment assumes that `services.UseInMemoryStorage()` was
registered. Create a fresh scope per delivery. `eventId` must be stable across
retries and unique for that bot; `fingerprint` must represent the immutable
event content. Addressing must match the adapter's session identity.

```csharp
using var scope = provider.CreateScope();
var sessions = scope.ServiceProvider.GetRequiredService<InMemorySessionsStore>();
var resolver = scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Update>>();
bool processed = await sessions.ExecuteUpdateAsync(
    eventId, fingerprint, chatId, userId,
    cancellationToken => resolver.Handle(update), cancellationToken);
```

This stages session writes until successful completion, serializes updates to
one session and remembers successful event IDs. A duplicate returns `false`
without invoking the handler. An exception or cancellation before commit leaves
the previous session intact and permits retry with the same ID. Reusing an ID
with different content/addressing fails explicitly. All concurrent writers must
use this boundary; cross-session moves and nested calls are rejected.

This is **not distributed storage**. Containers in the tests share the very same
`InMemorySessionStorage` object. Independent processes cannot do that. Receipts
and gates are retained for the backend's lifetime and are not a bounded durable
inbox. There is no automatic retry, transport ordering or crash recovery.
Remote sends and other side effects are not rolled back: a failed handler may
send again on retry. Production distributed execution still requires a durable
atomic inbox/session/outbox implementation and an external delivery policy.
