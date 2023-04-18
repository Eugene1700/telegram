using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

internal class StateBuilder<TObj> : IMessageBuilder<TObj>, IStateBuilder<TObj>, ICallbackRowBuilder<TObj>
{
    private readonly State<TObj> _state;
    private readonly StateMachineBuilder<TObj> _stateMachineBuilder;
    private readonly Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj>>>> _bodyExits;
    private int _currentBodyIndex;

    public StateBuilder(State<TObj> state, StateMachineBuilder<TObj> stateMachineBuilder)
    {
        _state = state;
        _stateMachineBuilder = stateMachineBuilder;
        _bodyExits = new Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj>>>>();
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
        var i = _bodyExits.Count;
        _bodyExits.Add(i, new List<Action<ICallbackRowBuilderBase<TObj>>>());
        _currentBodyIndex = i;
        _state.CallbackBuilder.AddProvider((o, b) =>
        {
            var rowBuilder = b.Row();
            var exits = _bodyExits[i];
            foreach (var exit in exits)
            {
                exit(rowBuilder);
            }
            return Task.CompletedTask;
        });
        
        return this;
    }

    public ICallbacksBuilder<TObj> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj>, Task> provider)
    {
        _state.CallbackBuilder.AddProvider(provider);
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

    public ICallbackRowBuilder<TObj> ExitStateByCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<string>> commitExpr) where TQuery : class
    {
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.ExitStateByCallback(callbackId, callbackProvider, commitExpr);
        });
        return this;
    }

    public ICallbackRowBuilder<TObj> ExitStateByCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId)
    {
        Func<object, TObj, string, Task<string>> commitExpr = (_, _, _) => Task.FromResult(stateId);
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.ExitStateByCallback(callbackId, callbackProvider, commitExpr);
        });
        return this;
    }

    public ICallbackRowBuilder<TObj> ExitStateByCallback(CallbackDataWithCommand callbackDataWithCommand)
    {
        CallbackData CallbackProvider(TObj _) => callbackDataWithCommand;
        return ExitStateByCallback(CallbackProvider, callbackDataWithCommand.CommandDescriptor);
    }

    public ICallbackRowBuilder<TObj> ExitStateByCallback(Func<TObj, CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
    {
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.ExitStateByCallback(callbackProvider, telegramCommandDescriptor);
        });
        return this;
    }

    public IMessageBuilder<TObj> WithMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sender)
    {
        _state.SetMessage(messageProvider, sender);
        return this;
    }
}