using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class CallbackDataContainer
{
    private readonly Func<IEnumerable<IEnumerable<CallbackDataWithCommand>>> _builder;
    private readonly CallbackDataWithCommand[] _callbackData;

    public CallbackDataContainer(CallbackDataWithCommand callbackData)
    {
        _callbackData = new[] { callbackData };
    }

    public CallbackDataContainer(CallbackDataWithCommand[] callbackData)
    {
        _callbackData = callbackData;
    }

    public CallbackDataContainer(Func<IEnumerable<IEnumerable<CallbackDataWithCommand>>> builder)
    {
        _builder = builder;
    }

    public IEnumerable<IEnumerable<CallbackDataWithCommand>> Build()
    {
        if (_callbackData != null && _callbackData.Any())
            return new[] { _callbackData };

        if (_builder != null)
            return _builder();

        throw new InvalidOperationException();
    }

    public bool HasCommand(ITelegramCommandDescriptor descriptor)
    {
        if (_callbackData.Any(x => x.CommandDescriptor != null && x.CommandDescriptor.MatchCommand(descriptor)))
            return true;

        if (_builder != null)
        {
            var callbacks = _builder();
            return callbacks.Select(callback => callback.Any(x => x.CommandDescriptor != null && x.CommandDescriptor.MatchCommand(descriptor))).FirstOrDefault();
        }

        return false;
    }
}