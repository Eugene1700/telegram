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
        private readonly List<Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task>> _providers;

        private readonly Dictionary<int, CallbackBuilder<TObj, TStates, TCallbacks>> _bodyExits;
        private int _currentBodyIndex;
        private readonly List<MessageContainer<TObj, TStates, TCallbacks>> _containersMessages;
        private MessageContainer<TObj, TStates, TCallbacks> _currentMessageContainer;
        private bool _buildOnce = false;

        public StateMessagesBuilder()
        {
            _providers = new List<Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task>>();
            _containersMessages = new List<MessageContainer<TObj, TStates, TCallbacks>>();
            _bodyExits = new Dictionary<int, CallbackBuilder<TObj, TStates, TCallbacks>>();
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
                foreach (var provider in _providers)
                {
                    await provider(obj, this);
                }

                _buildOnce = true;
            }

            return _containersMessages.ToArray();
        }

        public void AddProvider(Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> messageFlowProvider)
        {
            _providers.Add(messageFlowProvider);
        }

        public void AddMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task>  sender)
        {
            var i = _bodyExits.Count;
            _bodyExits.Add(i, new CallbackBuilder<TObj, TStates, TCallbacks>());
            _currentBodyIndex = i;
            Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> provider = (o, b) =>
            {
                var builder = b as StateMessagesBuilder<TObj, TStates, TCallbacks>;
                builder?.WithMessage(messageProvider, sender, _bodyExits[i]);
                return Task.CompletedTask;
            };
            _providers.Add(provider);
        }

        public void AddRow()
        {
            _bodyExits[_currentBodyIndex].AddRow();
        }

        public void AddOnCallback<TQuery>(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            _bodyExits[_currentBodyIndex].AddOnCallback(callbackId, callbackProvider, handler, force);
        }

        public void AddKeyBoardProvider(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _bodyExits[_currentBodyIndex].AddProvider(provider);
        }

        public void AddExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _bodyExits[_currentBodyIndex].AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
            TStates stateId, bool force)
        {
            _bodyExits[_currentBodyIndex].AddNextFromCallback(callbackId, callbackProvider, stateId, force);
        }

        public IMessageBuilderBase<TObj, TStates, TCallbacks> WithMessage(Func<TObj, Task<string>> messageProvider,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            var newContainer = new MessageContainer<TObj, TStates, TCallbacks>(messageProvider, sender);
            _containersMessages.Add(newContainer);
            _currentMessageContainer = newContainer;
            return this;
        }

        void WithMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task>  sender,
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