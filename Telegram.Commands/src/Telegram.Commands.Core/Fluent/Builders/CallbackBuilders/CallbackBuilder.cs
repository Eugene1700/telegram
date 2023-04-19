using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

internal class CallbackBuilder<TObj>: ICallbackRowBuilderBase<TObj>
{
    private readonly List<Func<TObj, ICallbacksBuilderBase<TObj>, Task>> _providers;
    private CallbackDataContainerRow<TObj> _currentRow;
    private readonly List<CallbackDataContainerRow<TObj>> _containerRows;
    private bool _buildOnce = false;

    public CallbackBuilder()
    {
        _providers = new List<Func<TObj, ICallbacksBuilderBase<TObj>, Task>>();
        _containerRows = new List<CallbackDataContainerRow<TObj>>();
    }
    public void AddProvider(Func<TObj, ICallbacksBuilderBase<TObj>, Task> provider)
    {
        _providers.Add(provider);
    }

    public async Task<CallbackDataContainerRow<TObj>[]> Build(TObj obj, bool force = false)
    {
        if (_buildOnce && force)
        {
            _providers.Clear();
            _containerRows.Clear();
            _currentRow = null;
        }

        if (!_buildOnce)
        {
            foreach (var provider in _providers)
            {
                await provider(obj, this);
            }
        }

        return _containerRows.ToArray();
    }

    public ICallbackRowBuilderBase<TObj> Row()
    {
        var newContainer = new CallbackDataContainerRow<TObj>();
        _containerRows.Add(newContainer);
        _currentRow = newContainer;
        return this;
    }

    public ICallbackRowBuilderBase<TObj> OnCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<string>> handler) where TQuery : class
    {
        _currentRow.AddContainer(callbackId, callbackProvider, handler);
        return this;
    }

    public ICallbackRowBuilderBase<TObj> NextFromCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId)
    {
        Func<object, TObj, string, Task<string>> commitExpr = (_, _, _) => Task.FromResult(stateId);
        _currentRow.AddContainer(callbackId, callbackProvider, commitExpr);
        return this;
    }

    public ICallbackRowBuilderBase<TObj> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand)
    {
        CallbackData CallbackProvider(TObj _) => callbackDataWithCommand;
        return ExitFromCallback(CallbackProvider, callbackDataWithCommand.CommandDescriptor);
    }

    public ICallbackRowBuilderBase<TObj> ExitFromCallback(Func<TObj, CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
    {
        _currentRow.AddContainer(callbackProvider,
            telegramCommandDescriptor: telegramCommandDescriptor);
        return this;
    }
}