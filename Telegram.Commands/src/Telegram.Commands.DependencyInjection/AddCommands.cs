﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.DependencyInjection
{
    public static class TelegramDependencyExtensions
    {
        private static void AddTelegramCommands(this IServiceCollection services)
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

        private static void AddCommandFactory(this IServiceCollection services)
        {
            services.AddScoped<ITelegramCommandFactory, TelegramCommandFactory>();
        }

        private static void AddTelegramClient(this IServiceCollection services)
        {
            services.AddScoped<ITelegramBotClient, TelegramClient>();
        }
        
        private static void AddSesionManager(this IServiceCollection services)
        {
            services.AddScoped<SessionManager>();
        }

        public static void UseTelegramCommandsServices(this IServiceCollection services)
        {
            services.AddTelegramCommands();
            services.AddCommandFactory();
            services.AddTelegramClient();
        }
    }
}