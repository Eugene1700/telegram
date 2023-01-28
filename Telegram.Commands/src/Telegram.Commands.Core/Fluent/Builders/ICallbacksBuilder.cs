﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface ICallbacksBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> ExitStateByCallback(string text, string data, Func<string, TObj, Task<string>> commitExpr);

    ICallbacksBuilder<TObj> ExitStateByCallback<TCommand>(string text, string data)
        where TCommand : IQueryTelegramCommand<CallbackQuery>;
    
    ICallbacksBuilder<TObj> ExitStateByCallback(string text, string data, IStateBase<TObj> nextState);
    ICallbacksBuilder<TObj> ExitStateByCallback(Func<IEnumerable<IEnumerable<CallbackDataWithCommand>>> builder);

}