using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Vk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Model;

namespace Bot.Commands.DependencyInjection;

public static class VkDependencyExtensions
{
    public static void UseVkCommandsServices(this IServiceCollection services)
    {
        services.UseBotCommandsServices();
        services.TryAddScoped<IBotCommandResolver<GroupUpdate>, VkCommandService>();
    }
}
