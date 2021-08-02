using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;

namespace Telegram.Commands.DependencyInjection
{
    public class TelegramCommandFactory : ITelegramCommandFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TelegramCommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITelegramCommand<T> GetCommand<T>(T message, Type commandType)
        {
            return (ITelegramCommand<T>)_serviceProvider.GetService(commandType);
        }
    }
}