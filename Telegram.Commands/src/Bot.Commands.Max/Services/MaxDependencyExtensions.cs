using Bot.Commands.Max.Services;
using System.Reflection;
using Bot.Commands.Abstract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bot.Commands.DependencyInjection;

public static class MaxDependencyExtensions
{
    public static void UseMaxCommandsServices(this IServiceCollection services, IEnumerable<Assembly>? assemblies = null)
    {
        services.UseBotCommandsServices(assemblies);
        services.TryAddScoped<IBotCommandResolver<global::Max.Bot.Types.Update>, MaxCommandService>();
    }
}
