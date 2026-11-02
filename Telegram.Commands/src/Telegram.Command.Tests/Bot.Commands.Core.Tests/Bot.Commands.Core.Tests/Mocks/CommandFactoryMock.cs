using Microsoft.Extensions.DependencyInjection;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Tests;

public class CommandFactoryMock : ICommandFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CommandFactoryMock(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public object GetCommand(Type commandType)
    {
        return _serviceProvider.GetRequiredService(commandType);
    }
}