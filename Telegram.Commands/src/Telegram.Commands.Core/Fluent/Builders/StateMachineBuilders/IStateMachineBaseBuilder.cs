using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBaseBuilder<TObj, TStates>
    {
        IStateMachine<TStates> Build();
        IStateMachineBuilder<TObj, TStates> Exit<TQuery>(TStates stateId,
            Func<TQuery, TStates, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery: class;
    }
}