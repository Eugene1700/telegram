using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract.Commands
{
    public interface IQueryTelegramCommand<TQuery>
    {
        Task<ITelegramCommandExecutionResult> Execute(QueryContext<TQuery> query);
    }
}