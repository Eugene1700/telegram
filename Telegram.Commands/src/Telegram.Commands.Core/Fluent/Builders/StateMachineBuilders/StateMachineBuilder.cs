using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Messages;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    internal class StateMachineBuilder<TObj, TStates> : IStateMachineBuilder<TObj, TStates>
    {
        private readonly StateMachine<TObj, TStates> _stateMachine;

        public StateMachineBuilder()
        {
            _stateMachine = new StateMachine<TObj, TStates>();
        }

        private IStateBuilder<TObj, TStates> NewState(TStates stateId, StateType stateType,
            Func<object, TStates, TObj, ITelegramMessage[], Task> messagesSender, uint? durationInSec)
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
            Func<TQuery, TStates, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
        {
            _stateMachine.AddExit(stateId, finalizer);
            return this;
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, null, durationInSec);
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId,
            Func<object, TStates, TObj, ITelegramMessage[], Task> sender, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, sender, durationInSec);
        }
    }
}