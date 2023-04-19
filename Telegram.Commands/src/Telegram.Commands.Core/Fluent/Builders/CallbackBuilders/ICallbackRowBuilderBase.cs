using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbackRowBuilderBase<TObj>: ICallbacksBuilderBase<TObj>
{
    ICallbackRowBuilderBase<TObj> NextStateFromCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<string>> commitExpr) where TQuery : class;
    ICallbackRowBuilderBase<TObj> NextStateFromCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId);
    ICallbackRowBuilderBase<TObj> NextStateFromCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilderBase<TObj> NextStateFromCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);
}