using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    internal class CallbackDataContainerRowsBuilder<TObj, TStates>
    {
        private readonly Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> _provider;
        private readonly CallbackDataContainerRow<TObj, TStates> _container;

        internal CallbackDataContainerRowsBuilder(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _provider = provider;
            _container = null;
        }

        internal CallbackDataContainerRowsBuilder(string prefix)
        {
            _container = new CallbackDataContainerRow<TObj, TStates>(prefix);
            _provider = null;
        }

        internal async Task<bool> TryBuild(TStates state, TObj obj, ICallbacksBuilderBase<TObj, TStates> builder)
        {
            if (_provider == null)
                return false;
            await _provider(state, obj, builder);
            return true;
        }

        internal bool TryGetContainer(out CallbackDataContainerRow<TObj, TStates> container)
        {
            container = null;
            if (_container == null) return false;
            container = _container;
            return true;
        }

        internal void AddOnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider,
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler,
            bool force) where TQuery : class
        {
            _container.AddContainer(callbackProvider, handler, force);
        }

        public void ExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _container.AddContainer(callbackProvider, telegramCommandDescriptor);
        }
    }
}
