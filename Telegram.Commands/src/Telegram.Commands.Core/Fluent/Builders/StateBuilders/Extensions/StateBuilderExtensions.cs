using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders.Extensions
{
    public static class StateBuilderExtensions {
    
        public static IActivityBuilder<TObj, TStates>  Next<TObj, TStates>(this IStateBuilder<TObj, TStates> builder, 
            Func<Message, TStates, TObj, Task<TStates>> handler, 
            bool forceRerender = true)
        {
            return builder.Next(handler, forceRerender);
        }
    }
}