using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class StateMachine<TObj, TStates, TCallbacks> : IStateMachine<TStates> where TCallbacks : struct, Enum
{
    private readonly Dictionary<TStates, IState<TObj, TStates>> _states;
    private TStates _entryStateId;

    public StateMachine()
    {
        _states = new Dictionary<TStates, IState<TObj, TStates>>();
    }

    public State<TObj, TStates, TCallbacks> AddState(TStates stateId, StateType stateType, uint? durationInSec, Func<object, TObj, Task<ITelegramCommandExecutionResult>> finalizer)
    {
        State<TObj, TStates, TCallbacks> newState;
        switch (stateType)
        {
            case StateType.Entry:
                newState = new State<TObj, TStates, TCallbacks>(stateId, stateType, durationInSec);
                _entryStateId = stateId;
                break;
            case StateType.Body:
                newState = new State<TObj, TStates, TCallbacks>(stateId, stateType, durationInSec);
                break;
            case StateType.Finish:
                newState = new State<TObj, TStates, TCallbacks>(stateId, stateType, durationInSec, finalizer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null);
        }

        _states.Add(stateId, newState);
        return newState;
    }

    public State<TObj, TStates, TCallbacks> AddExit<TQuery>(TStates stateId,
        Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
    {
        Task<ITelegramCommandExecutionResult> FinalizerObj(object q, TObj o) => finalizer(q as TQuery, o);
        return AddState(stateId, StateType.Finish, null, FinalizerObj);
    }

    public IStateBase<TStates> GetState(TStates currentStateId)
    {
        return GetStateInternal(currentStateId);
    }

    public IState<TObj, TStates> GetStateInternal(TStates currentStateId)
    {
        return _states[currentStateId];
    }

    public IState<TObj, TStates> GetEntryState()
    {
        return _states[_entryStateId];
    }
}