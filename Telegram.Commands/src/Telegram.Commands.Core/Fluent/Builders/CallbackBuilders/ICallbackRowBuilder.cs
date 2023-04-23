using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbackRowBuilder<TObj, TStates, TCallbacks> : ICallbacksBuilder<TObj, TStates, TCallbacks>
{
    ICallbackRowBuilder<TObj, TStates, TCallbacks> OnCallback<TQuery>(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
    ICallbackRowBuilder<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force);
    ICallbackRowBuilder<TObj, TStates, TCallbacks> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilder<TObj, TStates, TCallbacks> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);
}

public interface ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> : ICallbacksBuilderForMessage<TObj, TStates, TCallbacks>
{
    ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> OnCallback<TQuery>(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
    ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force);
    ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);
}