using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal class StateMessagesBuilder<TObj, TStates> : IMessageBuilderBase<TObj, TStates>,
        ICallbackRowBuilderForMessage<TObj, TStates>
    {
        private readonly string _prefix;
        private readonly IState<TObj, TStates> _currentState;
        private readonly List<MessageFlowBuilder<TObj, TStates>> _messageFlowBuilders;
        private MessageFlowBuilder<TObj, TStates> _currMessageFlowBuilder;

        private readonly List<MessageContainer<TObj, TStates>> _containersMessages;
        private MessageContainer<TObj, TStates> _currentMessageContainer;
        private bool _buildOnce = false;

        public StateMessagesBuilder(string prefix, IState<TObj, TStates> currentState)
        {
            _prefix = prefix;
            _currentState = currentState;
            _messageFlowBuilders = new List<MessageFlowBuilder<TObj, TStates>>();
            _containersMessages = new List<MessageContainer<TObj, TStates>>();
        }

        public async Task<MessageContainer<TObj, TStates>[]> Build(TObj obj, bool force = true)
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

        public void AddProvider(Func<TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider)
        {
            var newBuilder = new MessageFlowBuilder<TObj, TStates>(messageFlowProvider);
            _messageFlowBuilders.Add(newBuilder);
        }

        public void AddMessage(Func<TObj, Task<string>> messageProvider,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            var newBuilder =
                new MessageFlowBuilder<TObj, TStates>($"{_prefix}mfb{_messageFlowBuilders.Count}", messageProvider,
                    sender, _currentState);
            _messageFlowBuilders.Add(newBuilder);
            _currMessageFlowBuilder = newBuilder;
        }

        public void AddRow()
        {
            _currMessageFlowBuilder.AddRow();
        }

        public void AddOnCallback<TQuery>(Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            _currMessageFlowBuilder.AddOnCallback(callbackProvider, handler, force);
        }

        public void AddKeyBoardProvider(Func<TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _currMessageFlowBuilder.AddProvider(provider);
        }

        public void AddExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currMessageFlowBuilder.AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(Func<TObj, CallbackData> callbackProvider,
            TStates stateId, bool force)
        {
            _currMessageFlowBuilder.AddNextFromCallback(callbackProvider, stateId, force);
        }

        public IMessageBuilderBase<TObj, TStates> WithMessage(Func<TObj, Task<string>> messageProvider,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            var newContainer =
                new MessageContainer<TObj, TStates>($"{_prefix}mc{_containersMessages.Count}", messageProvider, sender,
                    _currentState);
            _containersMessages.Add(newContainer);
            _currentMessageContainer = newContainer;
            return this;
        }

        private void WithMessage(Func<TObj, Task<string>> messageProvider,
            Func<object, TObj, ITelegramMessage, Task> sender,
            CallbackBuilder<TObj, TStates> callbackBuilder)
        {
            var newContainer =
                new MessageContainer<TObj, TStates>(messageProvider, sender, callbackBuilder);
            _containersMessages.Add(newContainer);
            _currentMessageContainer = newContainer;
        }

        public ICallbacksBuilderForMessage<TObj, TStates> WithCallbacks()
        {
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates> Row()
        {
            _currentMessageContainer.CallbackBuilder.Row();
            return this;
        }

        public ICallbacksBuilderForMessage<TObj, TStates> KeyBoard(
            Func<TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _currentMessageContainer.CallbackBuilder.Keyboard(provider);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates> OnCallback<TQuery>(Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<TStates>> handler, bool force)
            where TQuery : class
        {
            _currentMessageContainer.CallbackBuilder.OnCallback(callbackProvider, handler, force);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates> NextFromCallback(Func<TObj, CallbackData> callbackProvider, 
            TStates stateId,
            bool force)
        {
            _currentMessageContainer.CallbackBuilder.NextFromCallback(callbackProvider, stateId, force);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(
            CallbackDataWithCommand callbackDataWithCommand)
        {
            _currentMessageContainer.CallbackBuilder.ExitFromCallback(callbackDataWithCommand);
            return this;
        }

        public ICallbackRowBuilderForMessage<TObj, TStates> ExitFromCallback(
            Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currentMessageContainer.CallbackBuilder.ExitFromCallback(callbackProvider, telegramCommandDescriptor);
            return this;
        }
    }
}