﻿using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbackRowBuilder<TObj, TStates> : ICallbacksBuilder<TObj, TStates>
    {
        ICallbackRowBuilder<TObj, TStates> OnCallback<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider,
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler, bool force) where TQuery : class;
        ICallbackRowBuilder<TObj, TStates> NextFromCallback(Func<TStates, TObj, CallbackData> callbackProvider, TStates stateId, bool force);
        ICallbackRowBuilder<TObj, TStates> ExitFromCallback(CallbackDataWithCommand callbackDataWithCommand);
        ICallbackRowBuilder<TObj, TStates> ExitFromCallback(Func<TStates, TObj, CallbackData> callbackProvider,
            ITelegramCommandDescriptor telegramCommandDescriptor);
    }
}