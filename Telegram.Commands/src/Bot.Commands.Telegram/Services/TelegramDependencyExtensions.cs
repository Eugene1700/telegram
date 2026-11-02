using Bot.Commands.Abstract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using global::Telegram.Bot;
using Bot.Commands.Telegram.Services;

namespace Bot.Commands.DependencyInjection;

public static class TelegramDependencyExtensions
{
    public static void UseTelegramCommandsServices(this IServiceCollection services)
    {
        services.UseBotCommandsServices();
        services.TryAddScoped<ITelegramBotClient>(provider =>
            new TelegramBotClient(provider.GetRequiredService<IBotProfile>().Key));
        services.TryAddScoped<IBotCommandResolver<global::Telegram.Bot.Types.Update>, TelegramCommandService>();
    }
}
