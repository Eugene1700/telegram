using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{

    internal class MessageFlowBuilder<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
    {
        private readonly Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> _provider;
        private readonly MessageContainer<TObj, TStates, TCallbacks> _container;

        internal MessageFlowBuilder(Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _provider = provider;
            _container = null;
        }

        internal MessageFlowBuilder(Func<TObj, Task<string>> messageProvider,
            Func<object, TObj, ITelegramMessage, Task> sendMessageProvider)
        {
            _provider = null;
            _container = new MessageContainer<TObj, TStates, TCallbacks>(messageProvider, sendMessageProvider);
        }

        public void AddRow()
        {
            _container.CallbackBuilder.AddRow();
        }

        public void AddOnCallback<TQuery>(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            _container.CallbackBuilder.AddOnCallback(callbackId, callbackProvider, handler, force);
        }

        public void AddProvider(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _container.CallbackBuilder.AddProvider(provider);
        }

        public void AddExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _container.CallbackBuilder.AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider,
            TStates stateId, bool force)
        {
            _container.CallbackBuilder.AddNextFromCallback(callbackId, callbackProvider, stateId, force);
        }

        public async Task<bool> TryBuild(TObj obj, StateMessagesBuilder<TObj, TStates, TCallbacks> builder)
        {
            if (_provider == null)
                return false;
            await _provider(obj, builder);
            return true;
        }

        public bool TryGetContainer(out MessageContainer<TObj, TStates, TCallbacks> container)
        {
            container = null;
            if (_container == null) return false;
            container = _container;
            return true;
        }
    }
}