using System;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands;

namespace SimpleHandlers.Services;

public sealed class ProfileFunnelMatcher : ICommandMatcher
{
    public Type CommandType => typeof(SharedProfileCommand);
    public bool IsMatch<TQuery>(QueryContext<TQuery> query)
        => !query.IsGroupMessage() && (string.Equals(query.GetData()?.Trim(), "register", StringComparison.OrdinalIgnoreCase)
            || string.Equals(query.GetData()?.Trim(), "хочу зарегистрироваться", StringComparison.OrdinalIgnoreCase));
}
