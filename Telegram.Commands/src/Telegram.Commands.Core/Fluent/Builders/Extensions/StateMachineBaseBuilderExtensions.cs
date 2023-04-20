using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions;

public static class StateMachineBaseBuilderExtensions
{
    public static IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Exit<TObj, TStates, TCallbacks>(this IStateMachineBaseBuilder<TObj, TStates, TCallbacks> builder, 
        TStates stateId, Func<Message, TObj, Task<ITelegramCommandExecutionResult>> finalizer)
    {
        return builder.Exit(stateId, finalizer);
    }
}