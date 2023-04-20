using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class CallbackDataContainerRow<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
{
    private readonly List<CallbackDataContainer<TObj, TStates, TCallbacks>> _containers;

    public CallbackDataContainerRow()
    {
        _containers = new List<CallbackDataContainer<TObj, TStates, TCallbacks>>();
    }

    public bool HasCommand(ITelegramCommandDescriptor curComDesc)
    {
        return _containers.Any(x => x.HasCommand(curComDesc));
    }

    public CallbackDataContainer<TObj, TStates, TCallbacks> AddContainer<TQuery>(TCallbacks callbackId,
        Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<TStates>> handler)
        where TQuery : class
    {
        Task<TStates> Handle(object q, TObj o, string d) => handler(q as TQuery, o, d);

        var newContainer = new CallbackDataContainer<TObj, TStates, TCallbacks>(callbackId, callbackProvider, Handle);
        _containers.Add(newContainer);
        return newContainer;
    }

    public ReadOnlyCollection<CallbackDataContainer<TObj, TStates, TCallbacks>> GetContainers()
    {
        return _containers.AsReadOnly();
    }

    public CallbackDataContainer<TObj, TStates, TCallbacks> AddContainer(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor)
    {
        var newContainer =
            new CallbackDataContainer<TObj, TStates, TCallbacks>(callbackProvider, telegramCommandDescriptor);
        _containers.Add(newContainer);
        return newContainer;
    }

    public bool TryGetByKey(TCallbacks callbackKey, string hash, TObj obj,
        out CallbackDataContainer<TObj, TStates, TCallbacks> o)
    {
        o = null;
        var containers = _containers.Where(x => x.CallbackKey.ToString() == callbackKey.ToString()).ToArray();
        if (!containers.Any()) return false;
        o = containers.Length == 1 ? containers.Single() : containers.FirstOrDefault(x => x.GetHash(obj) == hash);
        return o != null;
    }
}