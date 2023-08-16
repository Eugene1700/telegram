using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{

    public interface ICallbackRowBuilderForMessage<TObj, TStates> : ICallbacksBuilderForMessage<TObj, TStates>
    {
        ICallbackRowBuilderForMessage<TObj, TStates> OnCallback<TQuery>(
            Func<TStates, TObj, CallbackData> callbackProvider,
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;

        ICallbackRowBuilderForMessage<TObj, TStates> NextFromCallback(
            Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force);

        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);

        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(
            Func<TStates, TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor);
    }
}