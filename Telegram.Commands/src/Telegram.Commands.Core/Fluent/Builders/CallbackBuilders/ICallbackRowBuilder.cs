using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbackRowBuilder<TObj> : ICallbacksBuilder<TObj>
{
    ICallbackRowBuilder<TObj> OnCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<string>> handler) where TQuery : class;
    ICallbackRowBuilder<TObj> NextFromCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId);
    ICallbackRowBuilder<TObj> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilder<TObj> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);
}