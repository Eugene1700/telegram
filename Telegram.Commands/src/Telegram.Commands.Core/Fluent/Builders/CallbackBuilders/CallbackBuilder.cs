using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

internal class CallbackBuilder<TObj, TStates, TCallbacks>: ICallbackRowBuilderBase<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
{
    private readonly List<Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task>> _providers;
    private CallbackDataContainerRow<TObj, TStates, TCallbacks> _currentRow;
    private readonly List<CallbackDataContainerRow<TObj, TStates, TCallbacks>> _containerRows;
    private bool _buildOnce = false;

    public CallbackBuilder()
    {
        _providers = new List<Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task>>();
        _containerRows = new List<CallbackDataContainerRow<TObj, TStates, TCallbacks>>();
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
        Func<object, TObj, string, Task<TStates>> commitExpr = (_, _, _) => Task.FromResult(stateId);
        _currentRow.AddContainer(callbackId, callbackProvider, commitExpr, force);
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
}