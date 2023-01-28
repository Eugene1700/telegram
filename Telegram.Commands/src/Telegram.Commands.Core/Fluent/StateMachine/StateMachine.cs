using System.Collections.Generic;
using System.Linq;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class StateMachine<TObj> : IStateMachine<TObj>
{
    private readonly Dictionary<int, IState<TObj>> _states;

    public StateMachine()
    {
        _states = new Dictionary<int, IState<TObj>>();
    }

    private int GetId()
    {
        return _states.Any() ? _states.Max(x => x.Key) + 1 : 0;
    }

    public State<TObj> AddState()
    {
        var id = GetId();
        var newState = new State<TObj>(id);
        _states.Add(id, newState);
        return newState;
    }

    public IStateBase<TObj> GetState(int currentStateId)
    {
        return GetCurrentStateInternal(currentStateId);
    }

    public IState<TObj> GetCurrentStateInternal(int currentStateId)
    {
        return _states[currentStateId];
    }
}