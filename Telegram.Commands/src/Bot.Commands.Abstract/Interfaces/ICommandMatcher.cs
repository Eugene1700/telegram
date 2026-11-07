using System;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Abstract.Interfaces;

/// <summary>Matches an entry into a command when no session or explicit slash command applies.</summary>
public interface ICommandMatcher<TQuery>
{
    /// <summary>An attributed command type, or an attributed base shared by swarm implementations.</summary>
    Type CommandType { get; }
    bool IsMatch(IQueryContext<TQuery> query);
}
