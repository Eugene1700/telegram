using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbackRowBuilderBase<TObj, TStates>: ICallbacksBuilderBase<TObj, TStates>
    {
        ICallbackRowBuilderBase<TObj, TStates> OnCallback<TQuery>(Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
        ICallbackRowBuilderBase<TObj, TStates> NextFromCallback(Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force);
        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor);
        ICallbackRowBuilderBase<TObj, TStates> Back<TQuery>(Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task> handler, bool force) where TQuery : class;
    }
}