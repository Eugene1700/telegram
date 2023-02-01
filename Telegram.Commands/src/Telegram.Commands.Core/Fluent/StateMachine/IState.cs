using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal interface IState<TObj> : IStateBase<TObj>
{
    Task SendMessage(TObj obj);
    Task<string> Commit<TQuery>(TQuery query, TObj obj);
    Task<string> CallbackCommit<TQuery>(TQuery query, TObj obj);
    bool CanNext(IQueryTelegramCommand<CallbackQuery> currentCommand);
    StateType GetStateType();
}