using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbackRowBuilderBase<TObj, TStates>: ICallbacksBuilderBase<TObj, TStates>
    {
        ICallbackRowBuilderBase<TObj, TStates> OnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider,
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
        ICallbackRowBuilderBase<TObj, TStates> NextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force);
        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor);
    }
}