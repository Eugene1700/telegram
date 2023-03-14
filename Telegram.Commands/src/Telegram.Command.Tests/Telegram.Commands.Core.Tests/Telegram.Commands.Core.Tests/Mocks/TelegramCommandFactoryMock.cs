﻿using Microsoft.Extensions.DependencyInjection;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Tests;

public class TelegramCommandFactoryMock : ITelegramCommandFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramCommandFactoryMock(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public object GetCommand(Type commandType)
    {
        return _serviceProvider.GetRequiredService(commandType);
    }
}