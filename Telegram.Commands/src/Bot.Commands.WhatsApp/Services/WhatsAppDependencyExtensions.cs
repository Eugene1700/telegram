using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.WhatsApp.Services;
using WhatsApp.Messaging.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bot.Commands.DependencyInjection;

public static class WhatsAppDependencyExtensions
{
    public static void UseWhatsAppCommandsServices(this IServiceCollection services)
    {
        services.UseBotCommandsServices();
        services.TryAddScoped<IBotCommandResolver<WhatsAppMessage>, WhatsAppCommandService>();
    }
}
