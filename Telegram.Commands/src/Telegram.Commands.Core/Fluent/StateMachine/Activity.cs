using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Messages;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal class Activity<TObj, TStates> : IActivity<TStates>
    {
        private readonly ConcurrentDictionary<TStates, IState<TObj, TStates>> _states;

        public Activity()
        {
            _states = new ConcurrentDictionary<TStates, IState<TObj, TStates>>();
        }

        public State<TObj, TStates> AddState(TStates stateId, StateType stateType,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender, uint? durationInSec,
            Func<object, TStates, TObj, Task<ITelegramCommandExecutionResult>> finalizer)
        {
            var newState = stateType switch
            {
                StateType.Body => new State<TObj, TStates>(stateId, stateType, sender, durationInSec),
                StateType.Finish => new State<TObj, TStates>(stateId, stateType, sender, durationInSec, finalizer),
                _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
            };

            if (!_states.TryAdd(stateId, newState))
                throw new InvalidOperationException($"Could not add state {stateId}");
            return newState;
        }

        public State<TObj, TStates> AddExit<TQuery>(TStates stateId,
            Func<TQuery, TStates, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
        {
            Task<ITelegramCommandExecutionResult> FinalizerObj(object q, TStates s, TObj o) => finalizer(q as TQuery, s, o);
            return AddState(stateId, StateType.Finish, null, null, FinalizerObj);
        }

        public IStateBase<TStates> GetState(TStates currentStateId)
        {
            return GetStateInternal(currentStateId);
        }

        public IState<TObj, TStates> GetStateInternal(TStates currentStateId)
        {
            return _states[currentStateId];
        }
    }
}