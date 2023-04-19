using System;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders.Extensions;

public static class StateMachineBuilderExtensions
{
    public static IStateBuilder<TObj> Entry<TObj, TEnum>(this IStateMachineBuilder<TObj> builder, TEnum stateId, uint? durationInSec = null) where TEnum: Enum
    {
        return builder.Entry(stateId.ToString(), durationInSec);
    }
    
    public static IStateBuilder<TObj> State<TObj, TEnum>(this IStateMachineBodyBuilder<TObj> builder, TEnum stateId) where TEnum: Enum
    {
        return builder.State(stateId.ToString());
    }
}