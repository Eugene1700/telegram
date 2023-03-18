using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface ICallbackRowBuilder<TObj> : ICallbacksBuilder<TObj>
{
    ICallbackRowBuilder<TObj> ExitStateByCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<string>> commitExpr) where TQuery : class;
    ICallbackRowBuilder<TObj> ExitStateByCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId);
    ICallbackRowBuilder<TObj> ExitStateByCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilder<TObj> ExitStateByCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);

}