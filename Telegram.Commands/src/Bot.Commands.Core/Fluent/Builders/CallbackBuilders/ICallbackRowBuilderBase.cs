using Bot.Commands.Abstract.Commands;
using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbackRowBuilderBase<TObj, TStates>: ICallbacksBuilderBase<TObj, TStates>
    {
        ICallbackRowBuilderBase<TObj, TStates> OnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider,
            Func<IQueryContext<TQuery>, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
        ICallbackRowBuilderBase<TObj, TStates> NextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force);
        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ICommandDescriptor commandDescriptor);

        /// <summary>Creates a button targeting the command's attributed name.</summary>
        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback<TCommand>(
            Func<TStates, TObj, CallbackData> callbackProvider) where TCommand : class
            => ExitFromCallback(callbackProvider,
                BotCommandExtensions.GetCommandInfo(typeof(TCommand))
                ?? throw new InvalidOperationException($"Command '{typeof(TCommand).FullName}' requires CommandAttribute."));

        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback<TCommand>(CallbackData callback) where TCommand : class
            => ExitFromCallback<TCommand>((state, obj) => callback);

        ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback<TCommand>(string text, string data = null) where TCommand : class
            => ExitFromCallback<TCommand>(new CallbackData { Text = text, CallbackText = data });
    }
}