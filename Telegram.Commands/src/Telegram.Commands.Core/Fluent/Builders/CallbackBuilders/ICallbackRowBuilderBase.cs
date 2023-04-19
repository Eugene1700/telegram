using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbackRowBuilderBase<TObj>: ICallbacksBuilderBase<TObj>
{
    ICallbackRowBuilderBase<TObj> OnCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<string>> handler) where TQuery : class;
    ICallbackRowBuilderBase<TObj> NextFromCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId);
    ICallbackRowBuilderBase<TObj> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilderBase<TObj> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);
}