using Bot.Commands.Abstract.Commands;
using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Core.Fluent.Builders.CallbackBuilders
{

    public interface ICallbackRowBuilderForMessage<TObj, TStates> : ICallbacksBuilderForMessage<TObj, TStates>
    {
        ICallbackRowBuilderForMessage<TObj, TStates> OnCallback<TQuery>(
            Func<TStates, TObj, CallbackData> callbackProvider,
            Func<IQueryContext<TQuery>, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;

        ICallbackRowBuilderForMessage<TObj, TStates> NextFromCallback(
            Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force);

        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);

        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(
            Func<TStates, TObj, CallbackData> callbackProvider,
            ICommandDescriptor commandDescriptor);

        /// <summary>Creates a button targeting the command's attributed name.</summary>
        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback<TCommand>(
            Func<TStates, TObj, CallbackData> callbackProvider) where TCommand : class
            => ExitFromCallback(callbackProvider,
                BotCommandExtensions.GetCommandInfo(typeof(TCommand))
                ?? throw new InvalidOperationException($"Command '{typeof(TCommand).FullName}' requires CommandAttribute."));

        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback<TCommand>(CallbackData callback) where TCommand : class
            => ExitFromCallback<TCommand>((state, obj) => callback);

        ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback<TCommand>(string text, string data = null) where TCommand : class
            => ExitFromCallback<TCommand>(new CallbackData { Text = text, CallbackText = data });
    }
}