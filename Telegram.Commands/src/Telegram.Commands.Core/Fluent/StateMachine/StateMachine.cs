using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Commands.Core.Fluent.Builders;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class StateMachine<TObj> : IStateMachine<TObj>
{
    private readonly Dictionary<string, IState<TObj>> _states;
    private string _entryStateId;
    private string _finishStateId;

    public StateMachine()
    {
        _states = new Dictionary<string, IState<TObj>>();
    }

    public State<TObj> AddState(string stateId, StateType stateType, uint? durationInSec)
    {
        var newState = new State<TObj>(stateId, stateType, durationInSec);
        _states.Add(stateId, newState);
        switch (stateType)
        {
            case StateType.Entry:
                _entryStateId = stateId;
                break;
            case StateType.Body:
                break;
            case StateType.Finish:
                _finishStateId = stateId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null);
        }
            ;
        return newState;
    }

    public IStateBase<TObj> GetState(string currentStateId)
    {
        return GetStateInternal(currentStateId);
    }

    public IState<TObj> GetStateInternal(string currentStateId)
    {
        return _states[currentStateId];
    }

    public IState<TObj> GetEntryState()
    {
        return _states[_entryStateId];
    }
}

internal enum StateType
{
    Entry,
    Body,
    Finish
}