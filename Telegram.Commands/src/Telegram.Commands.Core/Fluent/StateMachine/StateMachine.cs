using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal class StateMachine<TObj, TStates> : IStateMachine<TStates>
    {
        private readonly Dictionary<TStates, IState<TObj, TStates>> _states;

        public StateMachine()
        {
            _states = new Dictionary<TStates, IState<TObj, TStates>>();
        }

        public State<TObj, TStates> AddState(TStates stateId, StateType stateType, Func<object, TObj, ITelegramMessage[], Task> sender, uint? durationInSec, Func<object, TObj, Task<ITelegramCommandExecutionResult>> finalizer)
        {
            var newState = stateType switch
            {
                StateType.Body => new State<TObj, TStates>(stateId, stateType, sender, durationInSec),
                StateType.Finish => new State<TObj, TStates>(stateId, stateType, sender, durationInSec, finalizer),
                _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
            };

            _states.Add(stateId, newState);
            return newState;
        }

        public State<TObj, TStates> AddExit<TQuery>(TStates stateId,
            Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
        {
            Task<ITelegramCommandExecutionResult> FinalizerObj(object q, TObj o) => finalizer(q as TQuery, o);
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