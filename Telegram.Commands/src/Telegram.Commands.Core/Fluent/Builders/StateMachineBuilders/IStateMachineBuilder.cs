﻿using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBuilder<TObj, TStates, TCallbacks>: IStateMachineBaseBuilder<TObj, TStates, TCallbacks>
    {
        IStateBuilder<TObj, TStates, TCallbacks> Entry(TStates stateId, uint? durationInSec = null);
        IStateBuilder<TObj, TStates, TCallbacks> Entry(TStates stateId, Func<object, TObj, ITelegramMessage[], Task> obj, uint? durationInSec = null);
    }
}