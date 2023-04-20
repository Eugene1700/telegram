using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders;

internal class StateBuilder<TObj, TStates, TCallbacks> : IMessageBuilder<TObj, TStates, TCallbacks>, IStateBuilder<TObj, TStates, TCallbacks>, ICallbackRowBuilder<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
{
    private readonly State<TObj, TStates, TCallbacks> _state;
    private readonly StateMachineBuilder<TObj, TStates, TCallbacks> _stateMachineBuilder;
    private readonly Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>> _bodyExits;
    private int _currentBodyIndex;

    public StateBuilder(State<TObj, TStates, TCallbacks> state, StateMachineBuilder<TObj, TStates, TCallbacks> stateMachineBuilder)
    {
        _state = state;
        _stateMachineBuilder = stateMachineBuilder;
        _bodyExits = new Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>>();
    }
    
    public IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next<TQuery>(Func<TQuery, TObj, Task<TStates>> handler) where TQuery : class
    {
        _state.SetHandler((q, o) => handler(q as TQuery,o));
        return _stateMachineBuilder;
    }

    public ICallbacksBuilder<TObj, TStates, TCallbacks> WithCallbacks()
    {
        return this;
    }

    public ICallbackRowBuilder<TObj, TStates, TCallbacks> Row()
    {
        var i = _bodyExits.Count;
        _bodyExits.Add(i, new List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>());
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

    public ICallbacksBuilder<TObj, TStates, TCallbacks> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider)
    {
        _state.CallbackBuilder.AddProvider(provider);
        return this;
    }

    public IStateBase<TStates> GetState()
    {
        return _state;
    }

    public IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next(TStates stateId)
    {
        _state.SetHandler((q, o) => Task.FromResult(stateId));
        return _stateMachineBuilder;
    }

    public IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Loop()
    {
        return Next(_state.Id);
    }

    public ICallbackRowBuilder<TObj, TStates, TCallbacks> OnCallback<TQuery>(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider, Func<TQuery, TObj, string, Task<TStates>> handler) where TQuery : class
    {
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.OnCallback(callbackId, callbackProvider, handler);
        });
        return this;
    }

    public ICallbackRowBuilder<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId, Func<TObj, CallbackData> callbackProvider, TStates stateId)
    {
        Func<object, TObj, string, Task<TStates>> commitExpr = (_, _, _) => Task.FromResult(stateId);
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.OnCallback(callbackId, callbackProvider, commitExpr);
        });
        return this;
    }

    public ICallbackRowBuilder<TObj, TStates, TCallbacks> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand)
    {
        CallbackData CallbackProvider(TObj _) => callbackDataWithCommand;
        return ExitFromCallback(CallbackProvider, callbackDataWithCommand.CommandDescriptor);
    }

    public ICallbackRowBuilder<TObj, TStates, TCallbacks> ExitFromCallback(Func<TObj, CallbackData> callbackProvider, ITelegramCommandDescriptor telegramCommandDescriptor)
    {
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.ExitFromCallback(callbackProvider, telegramCommandDescriptor);
        });
        return this;
    }

    public IMessageBuilder<TObj, TStates, TCallbacks> WithMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sender)
    {
        _state.SetMessage(messageProvider, sender);
        return this;
    }
}