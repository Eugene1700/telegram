using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class CallbackDataContainer<TObj>
{
    private readonly Func<TObj, CallbackDataWithCommand> _builder;
    private readonly ITelegramCommandDescriptor _telegramCommandDescriptor;
    private readonly Func<object, TObj, string, Task<string>> _committer;
    private static string _fcudKey = "fcUd";
    private const string _fcbidKey = "fcbId";

    public CallbackDataContainer(string callbackId, Func<TObj, CallbackData> provider,
        Func<object, TObj, string, Task<string>> committer = null)
    {
        CallbackDataWithCommand NewProvider(TObj o)
        {
            var callbackData = provider(o);
            return new CallbackDataWithCommand { CallbackMode = callbackData.CallbackMode, CallbackText = $"{_fcbidKey}={callbackId}&{_fcudKey}={callbackData.CallbackText}", Text = callbackData.Text };
        }

        CallbackKey = callbackId;
        _builder = NewProvider;
        _telegramCommandDescriptor = null;
        _committer = committer;
    }
    
    public CallbackDataContainer(Func<TObj, CallbackData> provider, ITelegramCommandDescriptor telegramCommandDescriptor)
    {
        Func<TObj, CallbackDataWithCommand> newProvider = (o) =>
        {
            var callbackData = provider(o);
            return new CallbackDataWithCommand
            {
                CallbackMode = callbackData.CallbackMode,
                CallbackText = callbackData.CallbackText,
                Text = callbackData.Text,
                CommandDescriptor = telegramCommandDescriptor
            };
        };

        CallbackKey = null;
        _builder = newProvider;
        _telegramCommandDescriptor = telegramCommandDescriptor;
        _committer = null;
    }

    public string CallbackKey { get; }

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