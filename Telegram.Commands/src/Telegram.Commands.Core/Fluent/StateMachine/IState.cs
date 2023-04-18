using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal interface IState<TObj> : IStateBase<TObj>
{
    Task SendMessage<TQuery>(TQuery currentQuery, TObj obj);
    Task<string> HandleQuery<TQuery>(TQuery query, TObj obj);
    Task<bool> IsCommandHandle(TObj obj, IQueryTelegramCommand<CallbackQuery> currentCommand);
    StateType GetStateType();
}