using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbackRowBuilder<TObj, TStates> : ICallbacksBuilder<TObj, TStates>
    {
        ICallbackRowBuilder<TObj, TStates> OnCallback<TQuery>(Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
        ICallbackRowBuilder<TObj, TStates> NextFromCallback(Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force);
        ICallbackRowBuilder<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
        ICallbackRowBuilder<TObj, TStates> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor);
        ICallbackRowBuilder<TObj, TStates> Back<TQuery>(Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task> handler, bool force) where TQuery : class;
    }

    public interface ICallbackRowBuilderForMessage<TObj, TStates> : ICallbacksBuilderForMessage<TObj, TStates>
    {
        ICallbackRowBuilderForMessage<TObj, TStates> OnCallback<TQuery>(Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
        ICallbackRowBuilderForMessage<TObj, TStates> NextFromCallback(Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force);
        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor);
    }
}