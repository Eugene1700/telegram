using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbackRowBuilderBase<TObj, TStates, TCallbacks>: ICallbacksBuilderBase<TObj, TStates, TCallbacks>
{
    ICallbackRowBuilderBase<TObj, TStates, TCallbacks> OnCallback<TQuery>(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
    ICallbackRowBuilderBase<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force);
    ICallbackRowBuilderBase<TObj, TStates, TCallbacks> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilderBase<TObj, TStates, TCallbacks> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);
}