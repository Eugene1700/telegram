using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders.Extensions;

public static class StateBuilderExtensions {
    public static IStateMachineBodyBuilder<TObj> NextState<TQuery, TObj, TEnum>(this IStateBuilderBase<TObj> builder, Func<TQuery, TObj, Task<TEnum>> commitStateExpr) where TEnum : Enum where TQuery : class
    {
        return builder.NextState<TQuery>(async (q, o) =>
        {
            var res = await commitStateExpr(q, o);
            return res.ToString();
        });
    }
    
    public static IStateMachineBodyBuilder<TObj>  NextState<TObj>(this IStateBuilderBase<TObj> builder, Func<Message, TObj, Task<string>> commitStateExpr)
    {
        return builder.NextState(commitStateExpr);
    }
    
    public static IStateMachineBodyBuilder<TObj> NextState<TObj, TEnum>(this IStateBuilderBase<TObj> builder, Func<Message, TObj, Task<TEnum>> handler) where TEnum : Enum
    {
        return NextState<Message, TObj, TEnum>(builder, handler);
    }

    public static IStateMachineBodyBuilder<TObj>  NextState<TObj, TEnum>(this IStateBuilderBase<TObj> builder, TEnum stateId)
    {
        return builder.NextState(stateId.ToString());
    }
}