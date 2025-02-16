using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    internal class ActivityBuilder<TObj, TStates> : IActivityPrimaryBuilder<TObj, TStates>
    {
        private readonly Activity<TObj, TStates> _activity;
        private Func<object,TStates,TObj,ITelegramMessage,Task> _sender = null;

        public ActivityBuilder()
        {
            _activity = new Activity<TObj, TStates>();
        }

        public IActivity<TStates> Build()
        {
            return _activity;
        }

        public IActivityBuilder<TObj, TStates> Exit<TQuery>(TStates stateId,
            Func<TQuery, TStates, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery : class
        {
            _activity.AddExit(stateId, finalizer);
            return this;
        }

        public IActivityBuilder<TObj, TStates> WithTransport(Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            _sender = sender;
            return this;
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, _sender, durationInSec);
        }

        public IStateBuilder<TObj, TStates> State(TStates stateId,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender, uint? durationInSec = null)
        {
            return NewState(stateId, StateType.Body, sender, durationInSec);
        }
        
        private IStateBuilder<TObj, TStates> NewState(TStates stateId, StateType stateType,
            Func<object, TStates, TObj, ITelegramMessage, Task> messagesSender, uint? durationInSec)
        {
            var newState = _activity.AddState(stateId, stateType, messagesSender, durationInSec, null);
            var entryStateBuilder = new StateBuilder<TObj, TStates>(newState, this);
            return entryStateBuilder;
        }
    }
}