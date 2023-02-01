using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

internal class StateBuilder<TObj> : IMessageBuilder<TObj>, IStateBuilder<TObj>, ICallbackRowBuilder<TObj>
{
    private readonly State<TObj> _state;
    private readonly StateMachineBuilder<TObj> _stateMachineBuilder;
    private CallbackDataContainerRow<TObj> _currentRow;

    public StateBuilder(State<TObj> state, StateMachineBuilder<TObj> stateMachineBuilder)
    {
        _state = state;
        _stateMachineBuilder = stateMachineBuilder;
    }
    
    public IStateMachineBodyBuilder<TObj> ExitState<TQuery>(Func<TQuery, TObj, Task<string>> commitStateExpr) where TQuery : class
    {
        _state.SetCommitter((q, o) => commitStateExpr(q as TQuery,o));
        return _stateMachineBuilder;
    }

    public ICallbacksBuilder<TObj> WithCallbacks()
    {
        return this;
    }

    public ICallbackRowBuilder<TObj> Row()
    {
        _currentRow = _state.AddCallbackRow();
        return this;
    }

    public IStateBase<TObj> GetState()
    {
        return _state;
    }

    public IStateMachineBodyBuilder<TObj> ExitState(string stateId)
    {
        _state.SetCommitter((q, o) => Task.FromResult(stateId));
        return _stateMachineBuilder;
    }

    public ICallbackRowBuilder<TObj> ExitStateByCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, Task<string>> commitExpr) where TQuery : class
    {
        var container = _currentRow.AddContainer(callbackId, callbackProvider, commitExpr);
        _state.AddIndex(callbackId, container);
        return this;
    }

    public ICallbackRowBuilder<TObj> ExitStateByCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId)
    {
        Func<object, TObj, Task<string>> commitExpr = (_, _) => Task.FromResult(stateId);
        var newContainer = _currentRow.AddContainer(callbackId, callbackProvider, commitExpr);
        _state.AddIndex(callbackId, newContainer);
        return this;
    }

    public ICallbackRowBuilder<TObj> ExitStateByCallback(CallbackDataWithCommand callbackDataWithCommand)
    {
        CallbackData CallbackProvider(TObj _) => callbackDataWithCommand;
        return ExitStateByCallback(CallbackProvider, callbackDataWithCommand.CommandDescriptor);
    }

    public ICallbackRowBuilder<TObj> ExitStateByCallback(Func<TObj, CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
    {
        _currentRow.AddContainer(callbackProvider,
            telegramCommandDescriptor: telegramCommandDescriptor);
        return this;
    }

    public IMessageBuilder<TObj> WithMessage(Func<TObj, string> messageProvider, IMessageSender<TObj> sender)
    {
        _state.SetMessage(messageProvider, sender);
        return this;
    }
}