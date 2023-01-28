using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

internal class StateMachineBuilder<TObj> : IStateMachineBuilder<TObj>
{
    private readonly StateMachine<TObj> _stateMachine;

    public StateMachineBuilder()
    {
        _stateMachine = new StateMachine<TObj>();
    }

    public IStateBuilder<TObj> NewState(string message)
    {
        var newState = _stateMachine.AddState();
        newState.SetMessage(message);
        var entryStateBuilder = new StateBuilder<TObj>(newState, this);
        return entryStateBuilder;
    }

    public IStateMachine<TObj> Finish()
    {
        return _stateMachine;
    }

    public State<TObj> GetNextState()
    {
        return _stateMachine.AddState();
    }
}