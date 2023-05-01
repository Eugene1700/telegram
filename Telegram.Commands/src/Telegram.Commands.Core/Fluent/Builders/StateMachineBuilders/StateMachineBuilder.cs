using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    internal class StateMachineBuilder<TObj, TStates, TCallbacks> : IStateMachineBuilder<TObj, TStates, TCallbacks>, IStateMachineBodyBuilder<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
    {
        private readonly StateMachine<TObj, TStates, TCallbacks> _stateMachine;

        public StateMachineBuilder()
        {
            _stateMachine = new StateMachine<TObj, TStates, TCallbacks>();
        }

        private IStateBuilder<TObj, TStates, TCallbacks> NewState(TStates stateId, StateType stateType,
            IMessagesSender<TObj> messagesSender, uint? durationInSec)
        {
            var newState = _stateMachine.AddState(stateId, stateType, messagesSender, durationInSec, null);
            var entryStateBuilder = new StateBuilder<TObj, TStates, TCallbacks>(newState, this);
            return entryStateBuilder;
        }

        public IStateBuilder<TObj, TStates, TCallbacks> Entry(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Entry, null, durationInSec);
        }

        public IStateBuilder<TObj, TStates, TCallbacks> Entry(TStates stateId, IMessagesSender<TObj> sender, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Entry, sender, durationInSec);
        }

        public IStateMachine<TStates> Build()
        {
            return _stateMachine;
        }

        public IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Exit<TQuery>(TStates stateId, Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
        {
            _stateMachine.AddExit(stateId, finalizer);
            return this;
        }

        public IStateBuilder<TObj, TStates, TCallbacks> State(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, null, durationInSec);
        }

        public IStateBuilder<TObj, TStates, TCallbacks> State(TStates stateId, IMessagesSender<TObj> sender, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, sender, durationInSec);
        }
    }
}