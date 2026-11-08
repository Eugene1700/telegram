using System.Threading.Tasks;
using System;
using System.Text.Json;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;
namespace Bot.Commands.Core.Fluent.Routing;

public abstract class RoutedFluentCommand<TData, TState> : FluentCommand<TData, TState>
    where TData : RoutedData<TState>
{
    protected sealed override async Task<(TData, TState)> Entry<TQuery>(IQueryContext<TQuery> query, TData data)
    {
        if (data != null && data.IsResuming)
        {
            data.IsResuming = false;
            if (data.RouteResult != null)
            {
                await OnReturn(query, data, data.ResumeAt, data.RouteResult);
                data.RouteResult = null;
            }
            return (data, data.ResumeAt);
        }
        return await Enter(query, data);
    }
    /// <summary>Create a new call instance. The return state and caller snapshot are chosen at runtime.</summary>
    protected Task<ICommandExecutionResult> Call<TChild, TChildData, TChildState>(
        IQueryContext<object> query, TData caller, TState resumeAt, TChildData child)
        where TChild : RoutedFluentCommand<TChildData, TChildState>
        where TChildData : RoutedData<TChildState>
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(child);
        ArgumentNullException.ThrowIfNull(query);
        if (ReferenceEquals(caller, child) || child.RouteId != null || child.Routes?.Count > 0)
            throw new ArgumentException("A call requires fresh child data.", nameof(child));
        var snapshot = JsonSerializer.SerializeToNode(caller).AsObject();
        // The collection belongs to the active session, never to an individual caller snapshot.
        snapshot.Remove(nameof(RoutedData<TState>.Routes));
        var routes = FluentRoutingExtensions.CopyRoutes(caller.Routes ?? new());
        if (caller.RouteId.HasValue && !routes.ContainsKey(caller.RouteId.Value))
            throw new InvalidOperationException("Parent route is missing from the session.");
        var route = new FluentRouteFrame {
            RouteId = Guid.NewGuid(), CallerCommand = this.GetCommandInfo().Name,
            DestinationCommand = BotCommandExtensions.GetFluentCommandInfo<TChild, TChildData, TChildState>().Name,
            ParentRouteId = caller.RouteId, ChatId = query.GetChatId(), UserId = query.GetFromId(),
            Data = JsonSerializer.SerializeToElement(snapshot), State = JsonSerializer.SerializeToElement(resumeAt) };
        routes.Add(route.RouteId, route);
        child.RouteId = route.RouteId;
        child.Routes = routes;
        child.IsResuming = false;
        child.RouteResult = null;
        return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.AheadFluent<TChild, TChildData, TChildState>(child, true, null));
    }

    /// <summary>Handle a returned value or cancellation before the caller's return-state prompt is sent.</summary>
    protected virtual Task OnReturn<TQuery>(IQueryContext<TQuery> query, TData data, TState resumeAt, FluentRouteResult result)
        => Task.CompletedTask;

    protected abstract Task<(TData, TState)> Enter<TQuery>(IQueryContext<TQuery> query, TData data);
}
