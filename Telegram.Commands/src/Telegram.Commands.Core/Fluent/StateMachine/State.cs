using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal class State<TObj, TStates> : IState<TObj, TStates>
    {
        private Func<object, TStates, TObj, Task<TStates>> _handler;
        private bool _forceNext = false;

        private readonly Func<object, TObj, Task<ITelegramCommandExecutionResult>> _finalizer;
        private readonly StateType _stateType;
        public TStates Id { get; }
        public uint? DurationInSec { get; }

        private readonly StateMessagesBuilder<TObj, TStates> _messagesBuilder;
        private readonly Func<object, TObj, ITelegramMessage[], Task> _sender;
        private bool _withoutAnswer;

        public State(TStates id, 
            StateType stateType, 
            Func<object, TObj, ITelegramMessage[], Task> sender, 
            uint? durationInSec,
            Func<object, TObj, Task<ITelegramCommandExecutionResult>> finalizer = null)
        {
            _stateType = stateType;
            Id = id;
            DurationInSec = durationInSec;
            _finalizer = finalizer;
            _messagesBuilder = new StateMessagesBuilder<TObj, TStates>(id.ToString(), this);
            _sender = sender;
            _withoutAnswer = false;
        }

        public async Task SendMessages<TQuery>(TQuery currentQuery, TObj obj)
        {
            var messageContainers = await _messagesBuilder.Build(Id, obj);
            var groupSending = _sender != null;
            var messages = new List<ITelegramMessage>();
            foreach (var messageContainer in messageContainers)
            {
                messages.Add(await messageContainer.SendMessage(currentQuery, Id, obj, !groupSending));
            }

            if (groupSending)
            {
                await _sender(currentQuery, obj, messages.ToArray());
            }
        }

        public async Task<(TStates, bool)> HandleQuery<TQuery>(TQuery query, TObj obj)
        {
            if (CallbackDataContainer<TObj, TStates>.IsCallback(query))
            {
                var (callbackId, _) = CallbackDataContainer<TObj, TStates>.ExtractData(query);
                if (!callbackId.StartsWith(_messagesBuilder.GetPrefix()))
                {
                    return (await _handler(query, Id, obj), _forceNext);
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

                throw new InvalidOperationException();
            }

            return (await _handler(query, Id, obj), _forceNext);
        }

        public async Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand)
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

        public void SetHandler(Func<object, TStates, TObj, Task<TStates>> handler, bool force)
        {
            _handler = handler;
            _forceNext = force;
        }

        public StateType GetStateType()
        {
            return _stateType;
        }

        public Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery query, TObj sessionObjectObject)
        {
            return _finalizer?.Invoke(query, sessionObjectObject);
        }

        public bool NeedAnswer => !_withoutAnswer;

        public void AddMessage(Func<TStates, TObj, Task<string>> messageProvider, Func<object, TStates, TObj, ITelegramMessage, Task>  sender)
        {
            _messagesBuilder.AddMessage(messageProvider, sender);
        }

        public void AddMessagesProvider(
            Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider)
        {
            _messagesBuilder.AddProvider(messageFlowProvider);
        }

        public void AddRow()
        {
            _messagesBuilder.AddRow();
        }

        public void AddOnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider, Func<TQuery, TStates, TObj,string,Task<TStates>> handler, bool force) where TQuery : class
        {
            _messagesBuilder.AddOnCallback(callbackProvider, handler, force);
        }

        public void AddKeyBoardProvider(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _messagesBuilder.AddKeyBoardProvider(provider);
        }

        public void AddExitFromCallback(Func<TStates, TObj,CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _messagesBuilder.AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(Func<TStates, TObj,CallbackData> callbackProvider, TStates stateId, bool force)
        {
            _messagesBuilder.AddNextFromCallback(callbackProvider, stateId, force);
        }

        public void ThisStateWithoutAnswer()
        {
            _withoutAnswer = true;
        }
    }
}