using System;
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
        Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<string>> commitExpr)
        where TQuery : class
    {
        Task<string> Committer(object q, TObj o, string d) => commitExpr(q as TQuery, o, d);

        var newContainer = new CallbackDataContainer<TObj>(callbackId, callbackProvider, Committer);
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
        var newContainer = new CallbackDataContainer<TObj>(callbackProvider, telegramCommandDescriptor);
        _containers.Add(newContainer);
        return newContainer;
    }

    public bool TryGetByKey(string callbackKey, out CallbackDataContainer<TObj> o)
    {
        o = _containers.FirstOrDefault(x => x.CallbackKey == callbackKey);
        return o != null;
    }
}