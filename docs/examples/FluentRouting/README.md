# Nested Fluent routing prototype

This offline example runs `OrderFlow -> AddressFlow -> CityFlow` and returns one
level at a time. The caller resumes at an explicit state with its original data
and the child's result. Cancelling a child resumes its caller without applying
the success callback. Cancelling an address therefore preserves the order's
previous address. A missing city must be filled before saving an address.

From the repository root (.NET 10 and Python 3):

```sh
dotnet build docs/examples/FluentRouting/FluentRouting.csproj
python docs/examples/FluentRouting/verify.py
```

The verifier submits synthetic Telegram messages and callbacks. It checks 17
updates with a new OS process for every update, then the same sequence in one
process using `UseInMemoryStorage()`. In-memory sessions disappear on restart;
file-backed sessions restore the nested callers. No bot token or API is used.
The project remains outside the main solution, like the other documentation
examples. It reuses the SurveyComparison demo host infrastructure.

## Application API

Use `RoutedData<TState>` for each flow's data and derive commands from
`RoutedFluentCommand<TData, TState>`. Implement `Enter` for initial entry and the
usual `StateMachine`. The base class handles returning to `ResumeAt` without
re-running initial entry or consuming the incoming answer in the parent.

The caller creates each route at runtime; there is no static route map.
Import `Bot.Commands.Core.Fluent.Routing`:

```csharp
// Inside the parent state machine:
.Route(OrderState.Address, (query, data) =>
    Call<AddressFlow, AddressData, AddressState>(
        query, data, OrderState.Review, new AddressData()))

// Inside the child; no router service is needed:
.ReturnRoute(CityState.Done, data => data.City)
.CancelRoute(CityState.Cancel)
```

These fragments belong to different flows. The parent handles the result with
its usual injected dependencies:

For conditional routing, `data.ReturnAsync(query, result)` and
`data.CancelAsync(query)` are extensions on `RoutedData<TState>`. No router service
registration or constructor dependency is needed.

```csharp
protected override Task OnReturn<TQuery>(
    IQueryContext<TQuery> query, OrderData data,
    OrderState resumeAt, FluentRouteResult result)
{
    if (!result.Cancelled)
    {
        var address = result.Get<AddressResult>();
        data.Address = address.Street + ", " + address.City;
    }
    return Task.CompletedTask;
}
```

Each call generates a RouteId and snapshots the caller's command name, data and
return state in the session's `Routes` collection. `ParentRouteId` links nested
calls. Caller snapshots exclude the collection, so it is stored only once.
One child can serve different callers and different return states.
The normal command pipeline resolves the caller under the current bot profile,
with authorization, chat checks and filters. Command definitions come from DI;
no CLR types are loaded from stored strings. Result contracts are checked by
`Get<TResult>()`. A cancellation has no success result.

The route instance is persistent **inside the session**, with the same lifetime
and commit boundary. Return/cancellation records `IsRealized` and the result in
that same collection. There is no `IRouteStore`, separate table or static registry.
Persistent session stores restore this collection after restart; in-memory storage
does not. Completed records remain until the session ends. This replaces the
experimental static-route and nested-snapshot APIs; their saved sessions require
migration or expiration before adopting this format.

## Execution and limits

`Next` handles `ExecuteResult.Fire` by re-entering the common `QueryHandler`.
`Next` saves the destination session first; `QueryHandler` then uses the usual
`GetCommand`, authorization, chat checks, filters and
`CommandExecutionService.Execute`. Behavior-command selection preserves the active
session, as in the original pipeline. There is no separate immediate executor.
A limit of 64 immediate transitions per update
prevents an unbounded loop.

Keep command names, state values, result contract names and data schemas
compatible with saved sessions, or migrate them. Caller data must be JSON
serializable. Nested calls remain subject to serializer depth and storage limits.
A new call requires fresh child data. OnReturn runs before the parent's prompt.

The example file store is sequential teaching infrastructure. Plain stores may
persist intermediate transitions; whole-update rollback requires an appropriate
transactional boundary. Tests here do not establish distributed concurrency,
duplicate suppression, transactional external side effects or live delivery.
Expiring/deleting the active session discards its call chain. Separate caller
TTL and cross-chat return are not supported.
