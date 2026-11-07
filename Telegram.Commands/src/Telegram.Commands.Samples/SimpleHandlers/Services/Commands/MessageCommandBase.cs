using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;
namespace SimpleHandlers.Services.Commands;
public abstract class MessageCommandBase : IQueryCommand<Tg.Message>, IQueryCommand<Mx.Message>
{
    public Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query) => ExecuteCore(query);
    public Task<ICommandExecutionResult> Execute(QueryContext<Mx.Message> query) => ExecuteCore(query);
    protected abstract Task<ICommandExecutionResult> ExecuteCore<TQuery>(QueryContext<TQuery> query);
}
