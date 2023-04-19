using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbackRowBuilder<TObj> : ICallbacksBuilder<TObj>
{
    ICallbackRowBuilder<TObj> NextStateFromCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<string>> commitExpr) where TQuery : class;
    ICallbackRowBuilder<TObj> NextStateFromCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId);
    ICallbackRowBuilder<TObj> NextStateFromCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilder<TObj> NextStateFromCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);
}