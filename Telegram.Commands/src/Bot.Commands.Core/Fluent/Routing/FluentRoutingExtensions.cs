using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Fluent.Routing;

/// <summary>Operates only on the route collection carried by the session. No independent storage or route registry.</summary>
public static class FluentRoutingExtensions
{
    public static Task<ICommandExecutionResult> ReturnAsync<TState, TResult>(
        this RoutedData<TState> child, IQueryContext<object> query, TResult result)
        => Task.FromResult(Resume(query, child, new FluentRouteResult {
            RouteId = child?.RouteId ?? Guid.Empty, Contract = typeof(TResult).FullName,
            Value = JsonSerializer.SerializeToElement(result) }));

    public static Task<ICommandExecutionResult> CancelAsync<TState>(this RoutedData<TState> child, IQueryContext<object> query)
        => Task.FromResult(Resume(query, child, new FluentRouteResult { RouteId = child?.RouteId ?? Guid.Empty,
            Cancelled = true, Value = JsonSerializer.SerializeToElement<object>(null) }));

    private static ICommandExecutionResult Resume<TState>(
        IQueryContext<object> query, RoutedData<TState> child, FluentRouteResult result)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (child?.RouteId == null) throw new InvalidOperationException("There is no caller to return to.");
        if (child.Routes == null || !child.Routes.TryGetValue(child.RouteId.Value, out var saved))
            throw new InvalidOperationException("Route does not exist in the current session.");
        if (saved.ChatId != query.GetChatId() || saved.UserId != query.GetFromId())
            throw new InvalidOperationException("Route belongs to another conversation.");
        var routes = CopyRoutes(child.Routes);
        var route = routes[child.RouteId.Value];
        if (route.IsRealized)
        {
            if (route.Result == null || route.Result.Cancelled != result.Cancelled ||
                route.Result.Contract != result.Contract || route.Result.Value.GetRawText() != result.Value.GetRawText())
                throw new InvalidOperationException("Route already has a different outcome.");
        }
        else
        {
            route.IsRealized = true;
            route.Result = result;
        }
        return CommandExecutionResult.ResumeRoute(new(route, route.Result, routes));
    }

    internal static Dictionary<Guid, FluentRouteFrame> CopyRoutes(Dictionary<Guid, FluentRouteFrame> routes)
        => JsonSerializer.Deserialize<Dictionary<Guid, FluentRouteFrame>>(JsonSerializer.Serialize(routes))
            ?? new Dictionary<Guid, FluentRouteFrame>();
}
