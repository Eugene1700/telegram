using System;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Abstract.Interfaces;

/// <summary>Selects an incoming command instead of name-based routing when matchers are configured.</summary>
public interface ICommandMatcher<TQuery>
{
    /// <summary>A concrete command type, or an abstract base whose concrete commands are candidates.</summary>
    Type CommandType { get; }
    bool IsMatch(IQueryContext<TQuery> query);
}

/// <summary>A matcher bound to a command type without a manually implemented type property.</summary>
public interface ICommandMatcher<TQuery, TCommand> : ICommandMatcher<TQuery>
    where TCommand : class
{
    Type ICommandMatcher<TQuery>.CommandType => typeof(TCommand);
}
