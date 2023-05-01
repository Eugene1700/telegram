using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal class State<TObj, TStates, TCallbacks> : IState<TObj, TStates> where TCallbacks : struct, Enum
    {
        private Func<object, TObj, Task<TStates>> _handler;
        private bool _forceNext = false;

        private readonly Func<object, TObj, Task<ITelegramCommandExecutionResult>> _finalizer;
        private readonly StateType _stateType;
        public TStates Id { get; }
        public uint? DurationInSec { get; }

        private readonly StateMessagesBuilder<TObj, TStates, TCallbacks> _messagesBuilder;
        private readonly IMessagesSender<TObj> _sender;

        public State(TStates id, 
            StateType stateType, 
            IMessagesSender<TObj> sender, 
            uint? durationInSec,
            Func<object, TObj, Task<ITelegramCommandExecutionResult>> finalizer = null)
        {
            _stateType = stateType;
            Id = id;
            DurationInSec = durationInSec;
            _finalizer = finalizer;
            _messagesBuilder = new StateMessagesBuilder<TObj, TStates, TCallbacks>();
            _sender = sender;
        }

        public async Task SendMessages<TQuery>(TQuery currentQuery, TObj obj)
        {
            var messageContainers = await _messagesBuilder.Build(obj);
            var groupSending = _sender != null;
            var messages = new List<ITelegramMessage>();
            foreach (var messageContainer in messageContainers)
            {
                messages.Add(await messageContainer.SendMessage(currentQuery, obj, !groupSending));
            }

            if (groupSending)
            {
                await _sender.Send(currentQuery, obj, messages.ToArray());
            }
        }

        public async Task<(TStates, bool)> HandleQuery<TQuery>(TQuery query, TObj obj)
        {
            if (CallbackDataContainer<TObj, TStates, TCallbacks>.IsCallback(query))
            {
                var messageContainers = await _messagesBuilder.Build(obj);
                foreach (var messageContainer in messageContainers)
                {
                    var (res, resHandle) = await messageContainer.TryHandleCallback(query, obj);
                    if (res)
                    {
                        return resHandle;
                    }
                }

                throw new InvalidOperationException();
            }

            return (await _handler(query, obj), _forceNext);
        }

        public async Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand)
        {
            var messageContainers = await _messagesBuilder.Build(obj);
            foreach (var messageContainer in messageContainers)
            {
                if (await messageContainer.IsCommandHandle(obj, currentCommand))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetHandler(Func<object, TObj, Task<TStates>> handler, bool force)
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

        public void AddMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sender)
        {
            _messagesBuilder.AddMessage(messageProvider, sender);
        }

        public void AddMessagesProvider(
            Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> messageFlowProvider)
        {
            _messagesBuilder.AddProvider(messageFlowProvider);
        }

        public void AddRow()
        {
            _messagesBuilder.AddRow();
        }

        public void AddOnCallback<TQuery>(TCallbacks callbackId, Func<TObj,CallbackData> callbackProvider, Func<TQuery,TObj,string,Task<TStates>> handler, bool force) where TQuery : class
        {
            _messagesBuilder.AddOnCallback(callbackId, callbackProvider, handler, force);
        }

        public void AddKeyBoardProvider(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _messagesBuilder.AddKeyBoardProvider(provider);
        }

        public void AddExitFromCallback(Func<TObj,CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _messagesBuilder.AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(TCallbacks callbackId, Func<TObj,CallbackData> callbackProvider, TStates stateId, bool force)
        {
            _messagesBuilder.AddNextFromCallback(callbackId, callbackProvider, stateId, force);
        }
    }
}