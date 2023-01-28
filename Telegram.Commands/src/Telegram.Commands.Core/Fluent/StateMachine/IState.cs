using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal interface IState<TObj> : IStateBase<TObj>
{
    Task<string> Commit(string message, TObj obj);
    Task<string> CallbackCommit(string data, TObj obj);
    IState<TObj> Next(string condition);
    bool CanNext(IQueryTelegramCommand<CallbackQuery> currentCommand);
}