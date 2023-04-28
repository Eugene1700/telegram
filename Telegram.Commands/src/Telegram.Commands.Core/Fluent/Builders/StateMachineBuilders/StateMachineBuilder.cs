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

        public IStateBuilder<TObj, TStates, TCallbacks> State(TStates stateId)
        {
            return NewState(stateId, StateType.Body);
        }

        private IStateBuilder<TObj, TStates, TCallbacks> NewState(TStates stateId, StateType stateType, uint? durationInSec = null)
        {
            var newState = _stateMachine.AddState(stateId, stateType, durationInSec, null);
            var entryStateBuilder = new StateBuilder<TObj, TStates, TCallbacks>(newState, this);
            return entryStateBuilder;
        }

        public IStateBuilder<TObj, TStates, TCallbacks> Entry(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Entry, durationInSec);
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
    }
}