using Microsoft.Extensions.DependencyInjection;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core
{
    public static class CompositionRoot
    {
        public static void AddSessionManager(this IServiceCollection services)
        {
            services.AddScoped<SessionManager>();
            services.AddScoped<ISessionManager>(x=>x.GetService<SessionManager>());
        }

        public static void AddResolver(this IServiceCollection services)
        {
            services.AddScoped<ITelegramCommandResolver, TelegramCommandService>();
        }
    }
}