using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface ICallbacksBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> ExitStateByCallback(string text, string data, Func<string, TObj, Task<string>> commitExpr);

    ICallbacksBuilder<TObj> ExitStateByCallback<TCommand>(string text, string data)
        where TCommand : IQueryTelegramCommand<CallbackQuery>;
    
    ICallbacksBuilder<TObj> ExitStateByCallback(string text, string data, IStateBase<TObj> nextState);
}