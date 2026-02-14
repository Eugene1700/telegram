using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract.Commands
{
    public interface ISessionTelegramCommand<TQuery, in TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> Execute(QueryContext<TQuery> query, TSessionObject sessionObject);
    }
}