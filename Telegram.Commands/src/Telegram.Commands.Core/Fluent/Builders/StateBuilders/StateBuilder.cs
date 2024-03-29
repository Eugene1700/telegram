﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Messages;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders
{
    internal class StateBuilder<TObj, TStates> : IMessageBuilder<TObj, TStates>, ICallbackRowBuilder<TObj, TStates>
    {
        private readonly State<TObj, TStates> _state;
        private readonly StateMachineBuilder<TObj, TStates> _stateMachineBuilder;

        public StateBuilder(State<TObj, TStates> state, StateMachineBuilder<TObj, TStates> stateMachineBuilder)
        {
            _state = state;
            _stateMachineBuilder = stateMachineBuilder;
        }

        public IStateMachineBuilder<TObj, TStates> Next<TQuery>(Func<TQuery, TStates, TObj, Task<TStates>> handler,
            bool force) where TQuery : class
        {
            _state.SetHandler((q, s, o) => handler(q as TQuery, s, o), force);
            return _stateMachineBuilder;
        }

        public ICallbacksBuilder<TObj, TStates> WithCallbacks()
        {
            return this;
        }

        public ICallbackRowBuilder<TObj, TStates> Row()
        {
            _state.AddRow();
            return this;
        }

        public ICallbacksBuilder<TObj, TStates> KeyBoard(
            Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider)
        {
            _state.AddKeyBoardProvider(provider);
            return this;
        }

        public IStateBase<TStates> GetState()
        {
            return _state;
        }

        public IStateMachineBuilder<TObj, TStates> Next(TStates stateId, bool force)
        {
            _state.SetHandler((q, s, o) => Task.FromResult(stateId), force);
            return _stateMachineBuilder;
        }

        public IStateMachineBuilder<TObj, TStates> Loop(bool force)
        {
            return Next(_state.Id, force);
        }

        public IStateMachineBuilder<TObj, TStates> FireAndForget()
        {
            _state.ThisStateWithoutAnswer();
            return _stateMachineBuilder;
        }

        public ICallbackRowBuilder<TObj, TStates> OnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider,
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler,
            bool force) where TQuery : class
        {
            _state.AddOnCallback(callbackProvider, handler, force);
            return this;
        }

        public ICallbackRowBuilder<TObj, TStates> NextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            TStates stateId,
            bool force)
        {
            _state.AddNextFromCallback(callbackProvider, stateId, force);
            return this;
        }

        public ICallbackRowBuilder<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand)
        {
            CallbackData CallbackProvider(TStates s, TObj _) => callbackDataWithCommand;
            return ExitFromCallback(CallbackProvider, callbackDataWithCommand.CommandDescriptor);
        }

        public ICallbackRowBuilder<TObj, TStates> ExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            _state.AddExitFromCallback(callbackProvider, telegramCommandDescriptor);
            return this;
        }

        public IMessageBuilder<TObj, TStates> WithMessage(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            _state.AddMessage(messageProvider, sender);
            return this;
        }

        public IStateBuilder<TObj, TStates> WithMessages(
            Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider)
        {
            _state.AddMessagesProvider(messageFlowProvider);
            return this;
        }
    }
}