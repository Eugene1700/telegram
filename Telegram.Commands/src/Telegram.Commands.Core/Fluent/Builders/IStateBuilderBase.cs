using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateBuilderBase<TObj>
{
    IStateBase<TObj> GetState();
    IStateMachineBodyBuilder<TObj> ExitState<TQuery>(Func<TQuery, TObj, Task<string>> commitStateExpr) where TQuery : class;
    IStateMachineBodyBuilder<TObj>  ExitState(string stateId);
}

public static class StateBuilderExtensions {
    public static IStateMachineBodyBuilder<TObj> ExitState<TQuery, TObj, TEnum>(this IStateBuilderBase<TObj> builder, Func<TQuery, TObj, Task<TEnum>> commitStateExpr) where TEnum : Enum where TQuery : class
    {
        return builder.ExitState<TQuery>(async (q, o) =>
        {
            var res = await commitStateExpr(q, o);
            return res.ToString();
        });
    }
    
    public static IStateMachineBodyBuilder<TObj>  ExitState<TObj>(this IStateBuilderBase<TObj> builder, Func<Message, TObj, Task<string>> commitStateExpr)
    {
        return builder.ExitState(commitStateExpr);
    }
    
    public static IStateMachineBodyBuilder<TObj> ExitState<TObj, TEnum>(this IStateBuilderBase<TObj> builder, Func<Message, TObj, Task<TEnum>> handler) where TEnum : Enum
    {
        return ExitState<Message, TObj, TEnum>(builder, handler);
    }

    public static IStateMachineBodyBuilder<TObj>  ExitState<TObj, TEnum>(this IStateBuilderBase<TObj> builder, TEnum stateId)
    {
        return builder.ExitState(stateId.ToString());
    }
}