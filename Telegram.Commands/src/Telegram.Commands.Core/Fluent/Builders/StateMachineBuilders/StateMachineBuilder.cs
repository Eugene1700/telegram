using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

internal class StateMachineBuilder<TObj> : IStateMachineBuilder<TObj>, IStateMachineBodyBuilder<TObj>
{
    private readonly StateMachine<TObj> _stateMachine;

    public StateMachineBuilder()
    {
        _stateMachine = new StateMachine<TObj>();
    }

    public IStateBuilder<TObj> NewState(string stateId)
    {
        return NewState(stateId, StateType.Body);
    }

    private IStateBuilder<TObj> NewState(string stateId, StateType stateType, uint? durationInSec = null)
    {
        var newState = _stateMachine.AddState(stateId, stateType, durationInSec, null);
        var entryStateBuilder = new StateBuilder<TObj>(newState, this);
        return entryStateBuilder;
    }

    public IStateBuilder<TObj> Entry(string stateId, uint? durationInSec = null)
    {
        return NewState(stateId, StateType.Entry, durationInSec);
    }

    public IStateMachine<TObj> Build()
    {
        return _stateMachine;
    }

    public IStateMachineBodyBuilder<TObj> Exit<TQuery>(string stateId, Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
    {
        _stateMachine.AddExit(stateId, finalizer);
        return this;
    }
}