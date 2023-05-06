using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    internal class CallbackBuilder<TObj, TStates, TCallbacks>: ICallbackRowBuilderBase<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
    {
        private CallbackDataContainerRow<TObj, TStates, TCallbacks> _currentRow;
        private readonly List<CallbackDataContainerRow<TObj, TStates, TCallbacks>> _containerRows;
        private readonly List<CallbackDataContainerRowsBuilder<TObj, TStates, TCallbacks>> _rowBuilders;
        private bool _buildOnce = false;
        private CallbackDataContainerRowsBuilder<TObj,TStates,TCallbacks> _currentRowBuilder;

        public CallbackBuilder()
        {
            _containerRows = new List<CallbackDataContainerRow<TObj, TStates, TCallbacks>>();
            _rowBuilders = new List<CallbackDataContainerRowsBuilder<TObj, TStates, TCallbacks>>();
        }
        public void AddProvider(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            var newRowBuilder = new CallbackDataContainerRowsBuilder<TObj, TStates, TCallbacks>(provider);
            _rowBuilders.Add(newRowBuilder);
        }

        public async Task<CallbackDataContainerRow<TObj, TStates, TCallbacks>[]> Build(TObj obj, bool force = true)
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
                    if (await rowBuilder.TryBuild(obj, this)) continue;
                    if (rowBuilder.TryGetContainer(out var cont))
                    {
                        _containerRows.Add(cont);
                    }
                }

                _buildOnce = true;
            }

            return _containerRows.ToArray();
        }

        public ICallbackRowBuilderBase<TObj, TStates, TCallbacks> Row()
        {
            var newContainer = new CallbackDataContainerRow<TObj, TStates, TCallbacks>();
            _containerRows.Add(newContainer);
            _currentRow = newContainer;
            return this;
        }

        public ICallbacksBuilderBase<TObj, TStates, TCallbacks> Keyboard(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            AddProvider(provider);
            return this;
        }

        public ICallbackRowBuilderBase<TObj, TStates, TCallbacks> OnCallback<TQuery>(TCallbacks callbackId, 
            Func<TObj, CallbackData> callbackProvider, 
            Func<TQuery, TObj, string, Task<TStates>> handler,
            bool force) where TQuery : class
        {
            _currentRow.AddContainer(callbackId, callbackProvider, handler, force);
            return this;
        }

        public ICallbackRowBuilderBase<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId, 
            Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force)
        {
            Task<TStates> CommitExpr(object x, TObj y, string z) => Task.FromResult(stateId);
            _currentRow.AddContainer(callbackId, callbackProvider, (Func<object, TObj, string, Task<TStates>>)CommitExpr, force);
            return this;
        }

        public ICallbackRowBuilderBase<TObj, TStates, TCallbacks> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand)
        {
            CallbackData CallbackProvider(TObj _) => callbackDataWithCommand;
            return ExitFromCallback(CallbackProvider, callbackDataWithCommand.CommandDescriptor);
        }

        public ICallbackRowBuilderBase<TObj, TStates, TCallbacks> ExitFromCallback(Func<TObj, CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currentRow.AddContainer(callbackProvider,
                telegramCommandDescriptor: telegramCommandDescriptor);
            return this;
        }
        
        public void AddRow()
        {
            var newRowBuilder = new CallbackDataContainerRowsBuilder<TObj, TStates, TCallbacks>();
            _rowBuilders.Add(newRowBuilder);
            _currentRowBuilder = newRowBuilder;
        }

        public void AddOnCallback<TQuery>(TCallbacks callbackId, Func<TObj,CallbackData> callbackProvider, Func<TQuery,TObj,string,Task<TStates>> handler, bool force) where TQuery : class
        {
            _currentRowBuilder.AddOnCallback(callbackId, callbackProvider, handler, force);
        }
        public void AddExitFromCallback(Func<TObj,CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _currentRowBuilder.ExitFromCallback(callbackProvider, telegramCommandDescriptor);
        }

        public void AddNextFromCallback(TCallbacks callbackId, Func<TObj,CallbackData> callbackProvider, TStates stateId, bool force)
        {
            Task<TStates> Handle(object q, TObj obj, string s) => Task.FromResult(stateId);
            _currentRowBuilder.AddOnCallback(callbackId, callbackProvider,
                (Func<object, TObj, string, Task<TStates>>)Handle, force);
        }
    }
}