using Bot.Commands.Abstract.Commands;
using global::Telegram.Bot.Types;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Bot.Commands.Telegram.Extensions
{
    public static class StateBuilderExtensions {

        public static IStateMachineBuilder<TObj, TStates>  Next<TObj, TStates>(this IStateBuilder<TObj, TStates> builder,
            Func<IQueryContext<Message>, TStates, TObj, Task<TStates>> handler,
            bool force)
        {
            return builder.Next(handler, force);
        }
    }
}