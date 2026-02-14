using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Telegram.Commands.Realization.Extensions
{
    public static class StateMachineBaseBuilderExtensions
    {
        public static IStateMachineBuilder<TObj, TStates> Exit<TObj, TStates, TCallbacks>(this IStateMachineBaseBuilder<TObj, TStates> builder, 
            TStates stateId, Func<Message, TStates, TObj, Task<ITelegramCommandExecutionResult>> finalizer)
        {
            return builder.Exit(stateId, finalizer);
        }
    }
}