using Bot.Commands.Abstract.Interfaces;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using global::Telegram.Bot;
using Bot.Commands.Telegram.Services;

namespace Bot.Commands.DependencyInjection;

public static class TelegramDependencyExtensions
{
    public static void UseTelegramCommandsServices(this IServiceCollection services, IEnumerable<Assembly>? assemblies = null)
    {
        services.UseBotCommandsServices(assemblies);
        services.TryAddScoped<ITelegramBotClient>(provider =>
            new TelegramBotClient(provider.GetRequiredService<IBotProfile>().Key));
        services.TryAddScoped<IBotCommandResolver<global::Telegram.Bot.Types.Update>, TelegramCommandService>();
    }
}
