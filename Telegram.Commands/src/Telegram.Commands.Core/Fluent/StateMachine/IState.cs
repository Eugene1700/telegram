using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal interface IState<TObj> : IStateBase<TObj>
{
    Task SendMessage<TQuery>(TQuery currentQuery, TObj obj);
    Task<string> HandleQuery<TQuery>(TQuery query, TObj obj);
    Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand);
    StateType GetStateType();
    Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery query, TObj sessionObjectObject);
}