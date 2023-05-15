using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    internal class StateMachineBuilder<TObj, TStates> : IStateMachineBuilder<TObj, TStates>, IStateMachineBodyBuilder<TObj, TStates>
    {
        private readonly StateMachine<TObj, TStates> _stateMachine;

        public StateMachineBuilder()
        {
            _stateMachine = new StateMachine<TObj, TStates>();
        }

        private IStateBuilder<TObj, TStates> NewState(TStates stateId, StateType stateType,
            Func<object, TObj, ITelegramMessage[], Task> messagesSender, uint? durationInSec)
        {
            var newState = _stateMachine.AddState(stateId, stateType, messagesSender, durationInSec, null);
            var entryStateBuilder = new StateBuilder<TObj, TStates>(newState, this);
            return entryStateBuilder;
        }

        public IStateBuilder<TObj, TStates> Entry(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Entry, null, durationInSec);
        }

        public IStateBuilder<TObj, TStates> Entry(TStates stateId, Func<object, TObj, ITelegramMessage[], Task> sender, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Entry, sender, durationInSec);
        }

        public IStateMachine<TStates> Build()
        {
            return _stateMachine;
        }

        public IStateMachineBodyBuilder<TObj, TStates> Exit<TQuery>(TStates stateId, Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
        {
            _stateMachine.AddExit(stateId, finalizer);
            return this;
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, null, durationInSec);
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId, Func<object, TObj, ITelegramMessage[], Task> sender, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, sender, durationInSec);
        }
    }
}