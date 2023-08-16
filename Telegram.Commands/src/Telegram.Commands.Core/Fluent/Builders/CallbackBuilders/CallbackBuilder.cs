using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    internal class CallbackBuilder<TObj, TStates>: ICallbackRowBuilderBase<TObj, TStates>
    {
        private readonly string _prefix;
        private readonly IState<TObj, TStates> _currentState;
        private CallbackDataContainerRow<TObj, TStates> _currentRow;
        private readonly List<CallbackDataContainerRow<TObj, TStates>> _containerRows;
        private readonly List<CallbackDataContainerRowsBuilder<TObj, TStates>> _rowBuilders;
        private bool _buildOnce = false;
        private CallbackDataContainerRowsBuilder<TObj,TStates> _currentRowBuilder;

        public CallbackBuilder(string prefix, IState<TObj, TStates> currentState)
        {
            _prefix = prefix;
            _currentState = currentState;
            _containerRows = new List<CallbackDataContainerRow<TObj, TStates>>();
            _rowBuilders = new List<CallbackDataContainerRowsBuilder<TObj, TStates>>();
        }
        public void AddProvider(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            var newRowBuilder = new CallbackDataContainerRowsBuilder<TObj, TStates>(provider);
            _rowBuilders.Add(newRowBuilder);
        }

        public async Task<CallbackDataContainerRow<TObj, TStates>[]> Build(TStates state, TObj obj, bool force = true)
        {
            if (force)
            {
                _containerRows.Clear();
                _currentRow = null;
            }

            if (!_buildOnce || force)
            {
                foreach (var rowBuilder in _rowBuilders)
                {
                    if (await rowBuilder.TryBuild(state, obj, this)) continue;
                    if (rowBuilder.TryGetContainer(out var cont))
                    {
                        _containerRows.Add(cont);
                    }
                }

                _buildOnce = true;
            }

            return _containerRows.ToArray();
        }

        public ICallbackRowBuilderBase<TObj, TStates> Row()
        {
            var newPrefix = $"{_prefix}p{_containerRows.Count}";
            var newContainer = new CallbackDataContainerRow<TObj, TStates>(newPrefix);
            _containerRows.Add(newContainer);
            _currentRow = newContainer;
            return this;
        }

        public ICallbacksBuilderBase<TObj, TStates> Keyboard(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            AddProvider(provider);
            return this;
        }

        public ICallbackRowBuilderBase<TObj, TStates> OnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider, 
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler,
            bool force) where TQuery : class
        {
            _currentRow.AddContainer(callbackProvider, handler, force);
            return this;
        }

        public ICallbackRowBuilderBase<TObj, TStates> NextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force)
        {
            Task<TStates> CommitExpr(object x, TStates s, TObj y, string z) => Task.FromResult(stateId);
            _currentRow.AddContainer(callbackProvider, (Func<object, TStates, TObj, string, Task<TStates>>)CommitExpr, force);
            return this;
        }

        public ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand)
        {
            CallbackData CallbackProvider(TStates s, TObj _) => callbackDataWithCommand;
            return ExitFromCallback(CallbackProvider, callbackDataWithCommand.CommandDescriptor);
        }

        public ICallbackRowBuilderBase<TObj, TStates> ExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currentRow.AddContainer(callbackProvider,
                telegramCommandDescriptor: telegramCommandDescriptor);
            return this;
        }

        public void AddRow()
        {
            var newPrefix = $"{_prefix}s{_rowBuilders.Count}";
            var newRowBuilder = new CallbackDataContainerRowsBuilder<TObj, TStates>(newPrefix);
            _rowBuilders.Add(newRowBuilder);
            _currentRowBuilder = newRowBuilder;
        }

        public void AddOnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider, Func<TQuery, TStates, TObj,string,Task<TStates>> handler, bool force) where TQuery : class
        {
            _currentRowBuilder.AddOnCallback(callbackProvider, handler, force);
        }
        public void AddExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currentRowBuilder.ExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force)
        {
            Task<TStates> Handle(object q, TStates state, TObj obj, string s) => Task.FromResult(stateId);
            _currentRowBuilder.AddOnCallback(callbackProvider,
                (Func<object, TStates, TObj, string, Task<TStates>>)Handle, force);
        }
    }
}