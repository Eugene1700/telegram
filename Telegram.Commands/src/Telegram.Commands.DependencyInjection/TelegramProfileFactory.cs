using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.DependencyInjection
{
    public class TelegramProfileFactory : ITelegramProfileFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TelegramProfileFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITelegramBotProfile GetProfile(Type profileType)
        {
            return (ITelegramBotProfile)_serviceProvider.GetService(profileType);
        }
    }
}