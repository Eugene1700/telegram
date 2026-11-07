using Bot.Commands.Abstract.Commands;
using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent.StateMachine;

namespace Bot.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBaseBuilder<TObj, TStates>
    {
        IStateMachine<TStates> Build();
        IStateMachineBuilder<TObj, TStates> Exit<TQuery>(TStates stateId,
            Func<IQueryContext<TQuery>, TStates, TObj, Task<ICommandExecutionResult>> finalizer) where TQuery: class;
    }
}