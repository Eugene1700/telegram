using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{

    internal class CallbackDataContainerRowsBuilder<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
    {
        private readonly Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> _provider;
        private readonly CallbackDataContainerRow<TObj, TStates, TCallbacks> _container;

        internal CallbackDataContainerRowsBuilder(
            Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _provider = provider;
            _container = null;
        }

        internal CallbackDataContainerRowsBuilder()
        {
            _container = new CallbackDataContainerRow<TObj, TStates, TCallbacks>();
            _provider = null;
        }

        internal async Task<bool> TryBuild(TObj obj, ICallbacksBuilderBase<TObj, TStates, TCallbacks> builder)
        {
            if (_provider == null)
                return false;
            await _provider(obj, builder);
            return true;
        }

        internal bool TryGetContainer(out CallbackDataContainerRow<TObj, TStates, TCallbacks> container)
        {
            container = null;
            if (_container == null) return false;
            container = _container;
            return true;
        }

        internal void AddOnCallback<TQuery>(TCallbacks callbackId,
            Func<TObj, CallbackData> callbackProvider,
            Func<TQuery, TObj, string, Task<TStates>> handler,
            bool force) where TQuery : class
        {
            _container.AddContainer(callbackId, callbackProvider, handler, force);
        }

        public void ExitFromCallback(Func<TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _container.AddContainer(callbackProvider, telegramCommandDescriptor);
        }
    }
}
