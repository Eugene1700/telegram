using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;

namespace Bot.Commands.Core.Fluent.StateMachine
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

        internal MessageFlowBuilder(string prefix, Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<IQueryContext<object>, TStates, TObj, IBotMessage, Task> sendMessageProvider, IState<TObj, TStates> currentState)
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
            Func<IQueryContext<TQuery>, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            _container.CallbackBuilder.AddOnCallback(callbackProvider, handler, force);
        }

        public void AddProvider(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _container.CallbackBuilder.AddProvider(provider);
        }

        public void AddExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ICommandDescriptor commandDescriptor)
        {
            _container.CallbackBuilder.AddExitFromCallback(callbackProvider, commandDescriptor);
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