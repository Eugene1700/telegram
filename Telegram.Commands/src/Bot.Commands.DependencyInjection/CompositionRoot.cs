using Microsoft.Extensions.DependencyInjection;
using Bot.Commands.Abstract.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Core
{
    public static class CompositionRoot
    {
        public static void AddSessionManager(this IServiceCollection services)
        {
            services.TryAddScoped<SessionManager>();
            services.TryAddScoped<ISessionManager>(x=>x.GetRequiredService<SessionManager>());
        }

        public static void AddResolver(this IServiceCollection services)
        {
            services.TryAddScoped<CommandExecutionService>();
        }
    }
}
