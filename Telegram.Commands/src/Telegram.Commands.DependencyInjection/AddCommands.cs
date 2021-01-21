using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Services;
using Telegram.Commands.Sessions;

namespace Telegram.Commands.DependencyInjection
{
    public static class TelegramDependencyExtensions
    {
        public static void AddTelegramCommands(this IServiceCollection services)
        {
            services.AddScoped<TelegramCommandService>();
            var commandType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p =>
                {
                    var attrLoc = p.GetCustomAttribute<CommandAttribute>();
                    var bp = p.GetInterfaces();
                    return p.IsClass && !p.IsAbstract &&
                           bp.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITelegramCommand<>))
                           && attrLoc != null;
                }).ToArray();
            foreach (var type in commandType)
            {
                services.AddScoped(type);
            }
        }

        public static void AddCommandFactory(this IServiceCollection services)
        {
            services.AddScoped<ITelegramCommandFactory, TelegramCommandFactory>();
        }
    }
}