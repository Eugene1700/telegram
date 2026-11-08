using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Bot.Commands.Core.Fluent.Routing;

public static class FluentRoutingBuilderExtensions
{
    public static IStateMachineBuilder<TData, TState> Route<TData, TState>(
        this IStateMachineBaseBuilder<TData, TState> builder, TState state,
        Func<IQueryContext<object>, TData, Task<ICommandExecutionResult>> route)
        where TData : RoutedData<TState>
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(route);
        return builder.Exit<object>(state, (query, _, data) => route(query, data));
    }

    /// <summary>Define a routing exit, including a conditional call or return. Evaluated when the state is reached.</summary>
    public static IStateMachineBuilder<TData, TState> Route<TData, TState>(
        this IStateMachineBaseBuilder<TData, TState> builder, TState state,
        Func<TData, ICommandExecutionResult> route)
        where TData : RoutedData<TState>
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(route);
        return builder.Exit<object>(state, (_, _, data) => Task.FromResult(route(data)));
    }

    /// <summary>Return a child result to its saved caller.</summary>
    public static IStateMachineBuilder<TData, TState> ReturnRoute<TData, TState, TResult>(
        this IStateMachineBaseBuilder<TData, TState> builder, TState state,
        Func<TData, TResult> result)
        where TData : RoutedData<TState>
    {
        ArgumentNullException.ThrowIfNull(result);
        return builder.Route(state, (query, data) => data.ReturnAsync(query, result(data)));
    }

    /// <summary>Resume the caller without applying the success result.</summary>
    public static IStateMachineBuilder<TData, TState> CancelRoute<TData, TState>(
        this IStateMachineBaseBuilder<TData, TState> builder, TState state)
        where TData : RoutedData<TState>
    {
        return builder.Route(state, (query, data) => data.CancelAsync(query));
    }
}
