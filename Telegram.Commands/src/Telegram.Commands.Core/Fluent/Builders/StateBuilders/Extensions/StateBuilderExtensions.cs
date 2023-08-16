using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders.Extensions
{
    public static class StateBuilderExtensions {
    
        public static IStateMachineBuilder<TObj, TStates>  Next<TObj, TStates>(this IStateBuilder<TObj, TStates> builder, 
            Func<Message, TStates, TObj, Task<TStates>> handler, 
            bool force)
        {
            return builder.Next(handler, force);
        }
    }
}