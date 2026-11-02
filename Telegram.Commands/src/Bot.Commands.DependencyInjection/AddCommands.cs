using System;
using System.Linq;
using System.Reflection;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bot.Commands.DependencyInjection;

public static class BotDependencyExtensions
{
    private static readonly Type[] CommandContracts =
    {
        typeof(IQueryCommand<>), typeof(ISessionCommand<,>), typeof(IBehaviorCommand<>)
    };

    public static void UseBotCommandsServices(this IServiceCollection services)
    {
        var commands = CommandTypeDiscovery.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters &&
                t.GetCustomAttribute<CommandAttribute>() != null &&
                t.GetInterfaces().Any(i => i.IsGenericType && CommandContracts.Contains(i.GetGenericTypeDefinition())));
        foreach (var command in commands) services.TryAddScoped(command);
        services.TryAddScoped<ICommandFactory, CommandFactory>();
        services.AddSessionManager();
        services.AddResolver();
    }
}
