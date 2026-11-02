using Bot.Commands.Max.Services;
using Bot.Commands.Abstract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bot.Commands.DependencyInjection;

public static class MaxDependencyExtensions
{
    public static void UseMaxCommandsServices(this IServiceCollection services)
    {
        services.UseBotCommandsServices();
        services.TryAddScoped<IBotCommandResolver<global::Max.Bot.Types.Update>, MaxCommandService>();
    }
}
