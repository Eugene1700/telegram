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
    private readonly Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>> _messages;
    private int _messagesIndex;

    public StateBuilder(State<TObj, TStates, TCallbacks> state, StateMachineBuilder<TObj, TStates, TCallbacks> stateMachineBuilder)
    {
        _state = state;
        _stateMachineBuilder = stateMachineBuilder;
        _bodyExits = new Dictionary<int, List<Action<ICallbackRowBuilderBase<TObj, TStates, TCallbacks>>>>();
    }
    
    public IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next<TQuery>(Func<TQuery, TObj, Task<TStates>> handler, bool force) where TQuery : class
    {
        _state.SetHandler((q, o) => handler(q as TQuery,o), force);
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
        _state.GetCurrentCallbackBuilder().AddProvider((o, b) =>
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
        _state.GetCurrentCallbackBuilder().AddProvider(provider);
        return this;
    }

    public IStateBase<TStates> GetState()
    {
        return _state;
    }

    public IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next(TStates stateId, bool force)
    {
        _state.SetHandler((q, o) => Task.FromResult(stateId), force);
        return _stateMachineBuilder;
    }

    public IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Loop(bool force)
    {
        return Next(_state.Id, force);
    }

    public ICallbackRowBuilder<TObj, TStates, TCallbacks> OnCallback<TQuery>(TCallbacks callbackId, 
        Func<TObj, CallbackData> callbackProvider, 
        Func<TQuery, TObj, string, Task<TStates>> handler,
        bool force) where TQuery : class
    {
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.OnCallback(callbackId, callbackProvider, handler, force);
        });
        return this;
    }

    public ICallbackRowBuilder<TObj, TStates, TCallbacks> NextFromCallback(TCallbacks callbackId, 
        Func<TObj, CallbackData> callbackProvider, 
        TStates stateId,
        bool force)
    {
        Task<TStates> Handle(object o, TObj obj, string s) => Task.FromResult(stateId);
        _bodyExits[_currentBodyIndex].Add((b) =>
        {
            b.OnCallback(callbackId, callbackProvider, (Func<object, TObj, string, Task<TStates>>)Handle, force);
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
        _state.AddMessage(messageProvider, sender);
        return this;
    }

    public IStateBuilder<TObj, TStates, TCallbacks> WithMessages(Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> messageFlowProvider)
    {
        throw new NotImplementedException();
    }
}

internal class MessageBuilder<TObj, TStates, TCallbacks>
{
    
}