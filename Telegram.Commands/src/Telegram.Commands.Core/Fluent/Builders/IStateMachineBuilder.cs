using System;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateMachineBuilder<TObj>: IStateMachineBaseBuilder<TObj>
{
    IStateBuilder<TObj> Entry(string stateId);
}

public static class StateMachineBuilderExtensions
{
    public static IStateBuilder<TObj> Entry<TObj, TEnum>(this IStateMachineBuilder<TObj> builder, TEnum stateId) where TEnum: Enum
    {
        return builder.Entry(stateId.ToString());
    }
    
    public static IStateBuilder<TObj> NewState<TObj, TEnum>(this IStateMachineBodyBuilder<TObj> builder, TEnum stateId) where TEnum: Enum
    {
        return builder.NewState(stateId.ToString());
    }
}

public interface IStateMachineBodyBuilder<TObj> : IStateMachineBaseBuilder<TObj>
{
    IStateBuilder<TObj> NewState(string stateId);
}

public interface IStateMachineBaseBuilder<TObj>
{
    IStateMachine<TObj> Finish(string stateId);
}

public static class StateMachineBaseBuilderExtensions
{
    public static IStateMachine<TObj> Finish<TObj, TEnum>(this IStateMachineBaseBuilder<TObj> builder, TEnum stateId) where TEnum:Enum
    {
        return builder.Finish(stateId.ToString());
    }
}