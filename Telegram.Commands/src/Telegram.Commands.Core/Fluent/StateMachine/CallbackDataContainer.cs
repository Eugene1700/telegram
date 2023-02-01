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
        Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<string>> commitExpr) where TQuery : class
    {
        Func<TObj, CallbackDataWithCommand> newProvider = (o) =>
        {
            var callbackData = callbackProvider(o);
            return new CallbackDataWithCommand
            {
                CallbackMode = callbackData.CallbackMode,
                CallbackText = $"{CallbackDataContainer<TObj>._fcbidKey}={callbackId}&{CallbackDataContainer<TObj>._fcudKey}={callbackData.CallbackText}",
                Text = callbackData.Text
            };
        };

        Task<string> Committer(object q, TObj o, string d) => commitExpr(q as TQuery, o, d);

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
    private readonly Func<object, TObj, string, Task<string>> _committer;
    public static string _fcudKey = "fcUd";
    internal const string _fcbidKey = "fcbId";

    public CallbackDataContainer(Func<TObj, CallbackDataWithCommand> provider,
        Func<object, TObj, string, Task<string>> committer = null, ITelegramCommandDescriptor telegramCommandDescriptor = null)
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

    public Task<string> Commit<TQuery>(TQuery query, TObj obj, string callbackUserData)
    {
        return _committer?.Invoke(query, obj, callbackUserData);
    }

    public bool HasCommand(ITelegramCommandDescriptor descriptor)
    {
        return _telegramCommandDescriptor != null && _telegramCommandDescriptor.MatchCommand(descriptor);
    }

    public static bool IsCallback<TQuery>(TQuery query)
    {
        var data = query.GetData();
        return data.Split("&").Any(x => x.StartsWith(_fcbidKey));
    }

    public static (string, string) ExtractData<TQuery>(TQuery query)
    {
        var data = query.GetData();
       var parametrs = data.Split("&").Select(x =>
        {
            var parts = x.Split("=");
            if (parts.Length < 2)
            {
                throw new InvalidOperationException();
            }
            return new KeyValuePair<string, string>(parts[0], parts[1]);
        }).ToArray();
       var callbackId = parametrs.SingleOrDefault(x => x.Key == _fcbidKey).Value;
       var userData = parametrs.SingleOrDefault(x => x.Key == _fcudKey).Value;
       return (callbackId, userData);
    }
}