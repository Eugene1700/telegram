using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.DependencyInjection
{
    public class TelegramCommandFactory : ITelegramCommandFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TelegramCommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetCommand(Type commandType)
        {
            return _serviceProvider.GetService(commandType);
        }
    }
}