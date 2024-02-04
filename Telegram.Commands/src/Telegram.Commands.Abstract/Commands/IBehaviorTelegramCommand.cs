using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract.Commands
{
    public interface IBehaviorTelegramCommand<TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query, TSessionObject sessionObject);
        
        Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand, TQuery query, TSessionObject sessionObject);

        Task<ITelegramCommandExecutionResult> Execute<TQuery>(
            ISessionTelegramCommand<TQuery, TSessionObject> currentCommand, TQuery query, TSessionObject sessionObject);

    }
}