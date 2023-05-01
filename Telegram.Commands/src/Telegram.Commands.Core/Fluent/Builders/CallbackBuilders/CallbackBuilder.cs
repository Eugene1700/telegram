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
        private readonly List<Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task>> _providers;
        private CallbackDataContainerRow<TObj, TStates, TCallbacks> _currentRow;
        private readonly List<CallbackDataContainerRow<TObj, TStates, TCallbacks>> _containerRows;
        private bool _buildOnce = false;
        private readonly Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>> _bodyExits;
        private int _currentBodyIndex;

        public CallbackBuilder()
        {
            _providers = new List<Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task>>();
            _containerRows = new List<CallbackDataContainerRow<TObj, TStates, TCallbacks>>();
            _bodyExits = new Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>>();

        }
        public void AddProvider(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
        {
            _providers.Add(provider);
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
                foreach (var provider in _providers)
                {
                    await provider(obj, this);
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

        public ICallbackRowBuilderBase<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider, TStates stateId, bool force)
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
            var i = _bodyExits.Count;
            _bodyExits.Add(i, new List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>());
            _currentBodyIndex = i;
            AddProvider((o, b) =>
            {
                var rowBuilder = b.Row();
                var exits = _bodyExits[i];
                foreach (var exit in exits)
                {
                    exit(rowBuilder);
                }
                return Task.CompletedTask;
            });
        }

        public void AddOnCallback<TQuery>(TCallbacks callbackId, Func<TObj,CallbackData> callbackProvider, Func<TQuery,TObj,string,Task<TStates>> handler, bool force) where TQuery : class
        {
            _bodyExits[_currentBodyIndex].Add((b) =>
            {
                b.OnCallback(callbackId, callbackProvider, handler, force);
            });
        }
        public void AddExitFromCallback(Func<TObj,CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _bodyExits[_currentBodyIndex].Add((b) =>
            {
                b.ExitFromCallback(callbackProvider, telegramCommandDescriptor);
            });
        }

        public void AddNextFromCallback(TCallbacks callbackId, Func<TObj,CallbackData> callbackProvider, TStates stateId, bool force)
        {
            Task<TStates> Handle(object o, TObj obj, string s) => Task.FromResult(stateId);
            _bodyExits[_currentBodyIndex].Add((b) =>
            {
                b.OnCallback(callbackId, callbackProvider, (Func<object, TObj, string, Task<TStates>>)Handle, force);
            });
        }
    }
}