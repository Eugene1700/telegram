using Bot.Commands.Abstract.Commands;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Bot.Commands.Telegram.Extensions
{
    public static class StateMachineBaseBuilderExtensions
    {
        public static IStateMachineBuilder<TObj, TStates> Exit<TObj, TStates, TCallbacks>(this IStateMachineBaseBuilder<TObj, TStates> builder, 
            TStates stateId, Func<IQueryContext<Message>, TStates, TObj, Task<ICommandExecutionResult>> finalizer)
        {
            return builder.Exit(stateId, finalizer);
        }
    }
}