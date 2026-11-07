using Bot.Commands.Abstract.Interfaces;
using Instagram.Messaging.Models;
using Bot.Commands.Instagram.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bot.Commands.DependencyInjection;

public static class InstagramDependencyExtensions
{
    public static void UseInstagramCommandsServices(this IServiceCollection services)
    {
        services.UseBotCommandsServices();
        services.TryAddScoped<IBotCommandResolver<InstagramMessagingEvent>, InstagramCommandService>();
    }
}
