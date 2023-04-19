using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class StateMachine<TObj> : IStateMachine<TObj>
{
    private readonly Dictionary<string, IState<TObj>> _states;
    private string _entryStateId;

    public StateMachine()
    {
        _states = new Dictionary<string, IState<TObj>>();
    }

    public State<TObj> AddState(string stateId, StateType stateType, uint? durationInSec, Func<object, TObj, Task<ITelegramCommandExecutionResult>> finalizer)
    {
        State<TObj> newState;
        switch (stateType)
        {
            case StateType.Entry:
                newState = new State<TObj>(stateId, stateType, durationInSec);
                _entryStateId = stateId;
                break;
            case StateType.Body:
                newState = new State<TObj>(stateId, stateType, durationInSec);
                break;
            case StateType.Finish:
                newState = new State<TObj>(stateId, stateType, durationInSec, finalizer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null);
        }

        _states.Add(stateId, newState);
        return newState;
    }

    public State<TObj> AddExit<TQuery>(string stateId,
        Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
    {
        Task<ITelegramCommandExecutionResult> FinalizerObj(object q, TObj o) => finalizer(q as TQuery, o);
        return AddState(stateId, StateType.Finish, null, FinalizerObj);
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