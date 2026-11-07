using Bot.Commands.Abstract.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Core.Fluent.StateMachine
{
    internal class StateMachine<TObj, TStates> : IStateMachine<TStates>
    {
        private readonly Dictionary<TStates, IState<TObj, TStates>> _states;

        public StateMachine()
        {
            _states = new Dictionary<TStates, IState<TObj, TStates>>();
        }

        public State<TObj, TStates> AddState(TStates stateId, StateType stateType,
            Func<IQueryContext<object>, TStates, TObj, IBotMessage[], Task> sender, uint? durationInSec,
            Func<IQueryContext<object>, TStates, TObj, Task<ICommandExecutionResult>> finalizer)
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
            Func<IQueryContext<TQuery>, TStates, TObj, Task<ICommandExecutionResult>> finalizer) where TQuery : class
        {
            Task<ICommandExecutionResult> FinalizerObj(IQueryContext<object> q, TStates s, TObj o) => finalizer(((IQueryContext<TQuery>)q), s, o);
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