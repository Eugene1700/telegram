using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{

    internal class MessageFlowBuilder<TObj, TStates>
    {
        private readonly Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> _provider;
        private readonly MessageContainer<TObj, TStates> _container;

        internal MessageFlowBuilder(Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> provider)
        {
            _provider = provider;
            _container = null;
        }

        internal MessageFlowBuilder(string prefix, Func<TStates, TObj, Task<IMessageText>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessage, Task> sendMessageProvider, IState<TObj, TStates> currentState)
        {
            _provider = null;
            _container =
                new MessageContainer<TObj, TStates>($"{prefix}mfb", messageProvider, sendMessageProvider, currentState);
        }

        public void AddRow()
        {
            _container.CallbackBuilder.AddRow();
        }

        public void AddOnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider,
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            _container.CallbackBuilder.AddOnCallback(callbackProvider, handler, force);
        }

        public void AddProvider(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _container.CallbackBuilder.AddProvider(provider);
        }

        public void AddExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _container.CallbackBuilder.AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            TStates stateId, bool force)
        {
            _container.CallbackBuilder.AddNextFromCallback(callbackProvider, stateId, force);
        }

        public async Task<bool> TryBuild(TStates state, TObj obj, StateMessagesBuilder<TObj, TStates> builder)
        {
            if (_provider == null)
                return false;
            await _provider(state, obj, builder);
            return true;
        }

        public bool TryGetContainer(out MessageContainer<TObj, TStates> container)
        {
            container = null;
            if (_container == null) return false;
            container = _container;
            return true;
        }
    }
}