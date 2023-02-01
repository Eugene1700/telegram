using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class CallbackDataContainerRow<TObj>
{
    private readonly List<CallbackDataContainer<TObj>> _containers;

    public CallbackDataContainerRow()
    {
        _containers = new List<CallbackDataContainer<TObj>>();
    }

    public bool HasCommand(ITelegramCommandDescriptor curComDesc)
    {
        return _containers.Any(x => x.HasCommand(curComDesc));
    }

    public CallbackDataContainer<TObj> AddContainer<TQuery>(string callbackId,
        Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, Task<string>> commitExpr) where TQuery : class
    {
        Func<TObj, CallbackDataWithCommand> newProvider = (o) =>
        {
            var callbackData = callbackProvider(o);
            return new CallbackDataWithCommand
            {
                CallbackMode = callbackData.CallbackMode,
                CallbackText = $"{callbackId} {callbackData.CallbackText}",
                Text = callbackData.Text
            };
        };

        Task<string> Committer(object q, TObj o) => commitExpr(q as TQuery, o);

        var newContainer = new CallbackDataContainer<TObj>(newProvider, Committer);
        _containers.Add(newContainer);
        return newContainer;
    }

    public ReadOnlyCollection<CallbackDataContainer<TObj>> GetContainers()
    {
        return _containers.AsReadOnly();
    }

    public CallbackDataContainer<TObj> AddContainer(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor)
    {
        Func<TObj, CallbackDataWithCommand> newProvider = (o) =>
        {
            var callbackData = callbackProvider(o);
            return new CallbackDataWithCommand
            {
                CallbackMode = callbackData.CallbackMode,
                CallbackText = callbackData.CallbackText,
                Text = callbackData.Text,
                CommandDescriptor = telegramCommandDescriptor
            };
        };
        var newContainer =
            new CallbackDataContainer<TObj>(newProvider, telegramCommandDescriptor: telegramCommandDescriptor);
        _containers.Add(newContainer);
        return newContainer;
    }
}

internal class CallbackDataContainer<TObj>
{
    private readonly Func<TObj, CallbackDataWithCommand> _builder;
    private readonly ITelegramCommandDescriptor _telegramCommandDescriptor;
    private readonly Func<object, TObj, Task<string>> _committer;

    public CallbackDataContainer(Func<TObj, CallbackDataWithCommand> provider,
        Func<object, TObj, Task<string>> committer = null, ITelegramCommandDescriptor telegramCommandDescriptor = null)
    {
        _builder = provider;
        _telegramCommandDescriptor = telegramCommandDescriptor;
        _committer = committer;
    }

    public CallbackDataWithCommand Build(TObj obj)
    {
        if (_builder != null)
            return _builder(obj);

        throw new InvalidOperationException();
    }

    public Task<string> Commit<TQuery>(TQuery query, TObj obj)
    {
        return _committer?.Invoke(query, obj);
    }

    public bool HasCommand(ITelegramCommandDescriptor descriptor)
    {
        return _telegramCommandDescriptor != null && _telegramCommandDescriptor.MatchCommand(descriptor);
    }
}