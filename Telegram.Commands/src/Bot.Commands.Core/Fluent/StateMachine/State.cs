using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Fluent.StateMachine
{
    internal class State<TObj, TStates> : IState<TObj, TStates>
    {
        private Func<IQueryContext<object>, TStates, TObj, Task<TStates>> _handler;
        private bool _forceNext = false;

        private readonly Func<IQueryContext<object>, TStates, TObj, Task<ICommandExecutionResult>> _finalizer;
        private readonly StateType _stateType;
        public TStates Id { get; }
        public uint? DurationInSec { get; }

        private readonly StateMessagesBuilder<TObj, TStates> _messagesBuilder;
        private readonly Func<IQueryContext<object>, TStates, TObj, IBotMessage[], Task> _sender;
        private bool _withoutAnswer;

        public State(TStates id,
            StateType stateType,
            Func<IQueryContext<object>, TStates, TObj, IBotMessage[], Task> sender,
            uint? durationInSec,
            Func<IQueryContext<object>, TStates, TObj, Task<ICommandExecutionResult>> finalizer = null)
        {
            _stateType = stateType;
            Id = id;
            DurationInSec = durationInSec;
            _finalizer = finalizer;
            _messagesBuilder = new StateMessagesBuilder<TObj, TStates>(id.ToString(), this);
            _sender = sender;
            _withoutAnswer = false;
        }

        public async Task SendMessages<TQuery>(IQueryContext<TQuery> currentQuery, TObj obj)
        {
            var messageContainers = await _messagesBuilder.Build(Id, obj);
            var groupSending = _sender != null;
            var messages = new List<IBotMessage>();
            foreach (var messageContainer in messageContainers)
            {
                messages.Add(await messageContainer.SendMessage(currentQuery, Id, obj, !groupSending));
            }

            if (groupSending)
            {
                await _sender((IQueryContext<object>)currentQuery, Id, obj, messages.ToArray());
            }
        }

        public async Task<(TStates, bool)> HandleQuery<TQuery>(IQueryContext<TQuery> query, TObj obj)
        {
            if (CallbackDataContainer<TObj, TStates>.IsCallback(query))
            {
                var (callbackId, _) = CallbackDataContainer<TObj, TStates>.ExtractData(query);
                if (!callbackId.StartsWith(_messagesBuilder.GetPrefix()))
                {
                    return (await _handler((IQueryContext<object>)query, Id, obj), _forceNext);
                }

                var messageContainers = await _messagesBuilder.Build(Id, obj);
                foreach (var messageContainer in messageContainers)
                {
                    var (res, resHandle) = await messageContainer.TryHandleCallback(query, Id, obj);
                    if (res)
                    {
                        return resHandle;
                    }
                }

                throw new InvalidOperationException("Could not handle query");
            }

            return (await _handler((IQueryContext<object>)query, Id, obj), _forceNext);
        }

        public async Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryCommand<TQuery> currentCommand)
        {
            var messageContainers = await _messagesBuilder.Build(Id, obj);
            foreach (var messageContainer in messageContainers)
            {
                if (await messageContainer.IsCommandHandle(Id, obj, currentCommand))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetHandler(Func<IQueryContext<object>, TStates, TObj, Task<TStates>> handler, bool force)
        {
            _handler = handler;
            _forceNext = force;
        }

        public StateType GetStateType()
        {
            return _stateType;
        }

        public Task<ICommandExecutionResult> Finalize<TQuery>(IQueryContext<TQuery> query, TObj sessionObjectObject)
        {
            return _finalizer?.Invoke((IQueryContext<object>)query, Id, sessionObjectObject);
        }

        public bool NeedAnswer => !_withoutAnswer;

        public void AddMessage<TQuery>(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<IQueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender)
        {
            _messagesBuilder.AddMessage(messageProvider, sender);
        }

        public void AddMessagesProvider(
            Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider)
        {
            _messagesBuilder.AddProvider(messageFlowProvider);
        }

        public void AddMessage(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<IQueryContext<object>, TStates, TObj, IBotMessage, Task> sender)
        {
            _messagesBuilder.AddMessage(messageProvider, sender);
        }

        public void AddRow()
        {
            _messagesBuilder.AddRow();
        }

        public void AddOnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider,
            Func<IQueryContext<TQuery>, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            _messagesBuilder.AddOnCallback(callbackProvider, handler, force);
        }

        public void AddKeyBoardProvider(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _messagesBuilder.AddKeyBoardProvider(provider);
        }

        public void AddExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ICommandDescriptor commandDescriptor)
        {
            _messagesBuilder.AddExitFromCallback(callbackProvider, commandDescriptor);
        }

        public void AddNextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force)
        {
            _messagesBuilder.AddNextFromCallback(callbackProvider, stateId, force);
        }

        public void ThisStateWithoutAnswer()
        {
            _withoutAnswer = true;
        }
    }
}
