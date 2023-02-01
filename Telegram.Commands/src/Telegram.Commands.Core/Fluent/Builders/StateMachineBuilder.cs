using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

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

    private IStateBuilder<TObj> NewState(string stateId, StateType stateType)
    {
        var newState = _stateMachine.AddState(stateId, stateType);
        var entryStateBuilder = new StateBuilder<TObj>(newState, this);
        return entryStateBuilder;
    }

    public IStateMachine<TObj> Finish(string stateId)
    {
        NewState(stateId, StateType.Finish);
        return _stateMachine;
    }
    

    public IStateBuilder<TObj> Entry(string stateId)
    {
        return NewState(stateId, StateType.Entry);
    }
}