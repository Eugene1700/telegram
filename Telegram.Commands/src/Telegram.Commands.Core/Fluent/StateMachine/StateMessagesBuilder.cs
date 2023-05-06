using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine{

    internal class StateMessagesBuilder<TObj, TStates, TCallbacks> : IMessageBuilderBase<TObj, TStates, TCallbacks>,
        ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
    {
        private readonly List<MessageFlowBuilder<TObj, TStates, TCallbacks>> _messageFlowBuilders;
        private MessageFlowBuilder<TObj, TStates, TCallbacks> _currMessageFlowBuilder;
        
        private readonly List<MessageContainer<TObj, TStates, TCallbacks>> _containersMessages;
        private MessageContainer<TObj, TStates, TCallbacks> _currentMessageContainer;
        private bool _buildOnce = false;

        public StateMessagesBuilder()
        {
            _messageFlowBuilders = new List<MessageFlowBuilder<TObj, TStates, TCallbacks>>();
            _containersMessages = new List<MessageContainer<TObj, TStates, TCallbacks>>();
        }

        public async Task<MessageContainer<TObj, TStates, TCallbacks>[]> Build(TObj obj, bool force = true)
        {
            if (force)
            {
                _containersMessages.Clear();
                _currentMessageContainer = null;
            }

            if (!_buildOnce || force)
            {
                foreach (var messageFlowBuilder in _messageFlowBuilders)
                {
                    if (!await messageFlowBuilder.TryBuild(obj, this))
                    {
                        if (messageFlowBuilder.TryGetContainer(out var cont))
                        {
                            _containersMessages.Add(cont);
                        }
                    }
                }

                _buildOnce = true;
            }

            return _containersMessages.ToArray();
        }

        public void AddProvider(Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> messageFlowProvider)
        {
            var newBuilder = new MessageFlowBuilder<TObj, TStates, TCallbacks>(messageFlowProvider);
            _messageFlowBuilders.Add(newBuilder);
        }

        public void AddMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task>  sender)
        {
            var newBuilder = new MessageFlowBuilder<TObj, TStates, TCallbacks>(messageProvider, sender);
            _messageFlowBuilders.Add(newBuilder);
            _currMessageFlowBuilder = newBuilder;
        }

        public void AddRow()
        {
            _currMessageFlowBuilder.AddRow();
        }

        public void AddOnCallback<TQuery>(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            _currMessageFlowBuilder.AddOnCallback(callbackId, callbackProvider, handler, force);
        }

        public void AddKeyBoardProvider(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _currMessageFlowBuilder.AddProvider(provider);
        }

        public void AddExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currMessageFlowBuilder.AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
            TStates stateId, bool force)
        {
            _currMessageFlowBuilder.AddNextFromCallback(callbackId, callbackProvider, stateId, force);
        }

        public IMessageBuilderBase<TObj, TStates, TCallbacks> WithMessage(Func<TObj, Task<string>> messageProvider,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            var newContainer = new MessageContainer<TObj, TStates, TCallbacks>(messageProvider, sender);
            _containersMessages.Add(newContainer);
            _currentMessageContainer = newContainer;
            return this;
        }

        private void WithMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task>  sender,
            CallbackBuilder<TObj, TStates, TCallbacks> callbackBuilder)
        {
            var newContainer =
                new MessageContainer<TObj, TStates, TCallbacks>(messageProvider, sender, callbackBuilder);
            _containersMessages.Add(newContainer);
            _currentMessageContainer = newContainer;
        }

        public ICallbacksBuilderForMessage<TObj, TStates, TCallbacks> WithCallbacks()
        {
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> Row()
        {
            _currentMessageContainer.CallbackBuilder.Row();
            return this;
        }

        public ICallbacksBuilderForMessage<TObj, TStates, TCallbacks> KeyBoard(
            Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _currentMessageContainer.CallbackBuilder.Keyboard(provider);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> OnCallback<TQuery>(TCallbacks callbackId,
            Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<TStates>> handler, bool force)
            where TQuery : class
        {
            _currentMessageContainer.CallbackBuilder.OnCallback(callbackId, callbackProvider, handler, force);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId,
            Func<TObj, CallbackData> callbackProvider, TStates stateId,
            bool force)
        {
            _currentMessageContainer.CallbackBuilder.NextFromCallback(callbackId, callbackProvider, stateId, force);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> ExitFromCallback(
            CallbackDataWithCommand callbackDataWithCommand)
        {
            _currentMessageContainer.CallbackBuilder.ExitFromCallback(callbackDataWithCommand);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> ExitFromCallback(
            Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currentMessageContainer.CallbackBuilder.ExitFromCallback(callbackProvider, telegramCommandDescriptor);
            return this;
        }
    }
}