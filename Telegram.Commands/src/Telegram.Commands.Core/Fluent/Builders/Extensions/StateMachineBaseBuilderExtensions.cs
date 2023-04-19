using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions;

public static class StateMachineBaseBuilderExtensions
{
    public static IStateMachineBodyBuilder<TObj> Exit<TObj, TEnum, TQuery>(this IStateMachineBaseBuilder<TObj> builder, 
        TEnum stateId, Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TEnum:Enum where TQuery : class
    {
        return builder.Exit(stateId.ToString(), finalizer);
    }
    
    public static IStateMachineBodyBuilder<TObj> Exit<TObj, TEnum>(this IStateMachineBaseBuilder<TObj> builder, 
        TEnum stateId, Func<Message, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TEnum:Enum
    {
        return builder.Exit(stateId.ToString(), finalizer);
    }
    
    public static IStateMachineBodyBuilder<TObj> Exit<TObj>(this IStateMachineBaseBuilder<TObj> builder, 
        string stateId, Func<Message, TObj, Task<ITelegramCommandExecutionResult>> finalizer)
    {
        return builder.Exit(stateId, finalizer);
    }
}