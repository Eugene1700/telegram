using Bot.Commands.Abstract.Commands;
using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;
using Bot.Commands.Core.Fluent.StateMachine;

namespace Bot.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    internal class StateMachineBuilder<TObj, TStates> : IStateMachineBuilder<TObj, TStates>
    {
        private readonly StateMachine<TObj, TStates> _stateMachine;

        public StateMachineBuilder()
        {
            _stateMachine = new StateMachine<TObj, TStates>();
        }

        private IStateBuilder<TObj, TStates> NewState(TStates stateId, StateType stateType,
            Func<IQueryContext<object>, TStates, TObj, IBotMessage[], Task> messagesSender, uint? durationInSec)
        {
            var newState = _stateMachine.AddState(stateId, stateType, messagesSender, durationInSec, null);
            var entryStateBuilder = new StateBuilder<TObj, TStates>(newState, this);
            return entryStateBuilder;
        }

        public IStateMachine<TStates> Build()
        {
            return _stateMachine;
        }

        public IStateMachineBuilder<TObj, TStates> Exit<TQuery>(TStates stateId,
            Func<IQueryContext<TQuery>, TStates, TObj, Task<ICommandExecutionResult>> finalizer) where TQuery : class
        {
            _stateMachine.AddExit(stateId, finalizer);
            return this;
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, null, durationInSec);
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId,
            Func<IQueryContext<object>, TStates, TObj, IBotMessage[], Task> sender, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, sender, durationInSec);
        }
    }
}