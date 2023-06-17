﻿using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders
{
    public interface IStateBuilder<TObj, TStates>
    {
        IStateBase<TStates> GetState();
        IStateMachineBuilder<TObj, TStates> Next<TQuery>(Func<TQuery, TObj, Task<TStates>> handler, bool force) where TQuery : class;
        IStateMachineBuilder<TObj, TStates> Next(TStates stateId, bool force);
        IStateMachineBuilder<TObj, TStates> Loop(bool force);
        IStateMachineBuilder<TObj, TStates> FireAndForget();
        IMessageBuilder<TObj, TStates> WithMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task> sender = null);
        IStateBuilder<TObj, TStates> WithMessages(Func<TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider);
    }

    public interface IStateBuilderBase<TObj, TStates>
    {
        IMessageBuilderBase<TObj, TStates> WithMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task> sender = null);
    }
}