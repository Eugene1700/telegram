using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBaseBuilder<TObj, TStates, TCallbacks>
    {
        IStateMachine<TStates> Build();
        IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Exit<TQuery>(TStates stateId,
            Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery: class;
    }
}