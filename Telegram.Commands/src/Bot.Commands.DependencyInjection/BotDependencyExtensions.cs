using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public static void UseBotCommandsServices(this IServiceCollection services, IEnumerable<Assembly> assemblies = null)
    {
        var selectedAssemblies = assemblies?.ToArray();
        var catalog = selectedAssemblies == null
            ? services.LastOrDefault(service => service.ServiceType == typeof(CommandCatalog))?.ImplementationInstance as CommandCatalog
              ?? new CommandCatalog()
            : new CommandCatalog(selectedAssemblies);
        if (selectedAssemblies == null)
            services.TryAddSingleton(catalog);
        else
            services.Replace(ServiceDescriptor.Singleton(catalog));

        var commands = catalog.GetCommandTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && CommandContracts.Contains(i.GetGenericTypeDefinition())));
        foreach (var command in commands) services.TryAddScoped(command);
        services.TryAddScoped<ICommandFactory, CommandFactory>();
        services.AddSessionManager();
        services.AddResolver();
    }
}
