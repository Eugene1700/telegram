using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders.Extensions;

public static class StateBuilderExtensions {
    
    public static IStateMachineBodyBuilder<TObj, TStates, TCallbacks>  Next<TObj, TStates, TCallbacks>(this IStateBuilderBase<TObj, TStates, TCallbacks> builder, Func<Message, TObj, Task<TStates>> handler)
    {
        return builder.Next(handler);
    }
}