using System;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.DependencyInjection
{
    public class CommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetCommand(Type commandType)
        {
            return _serviceProvider.GetService(commandType);
        }
    }
}