using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces.Commands
{
    public interface IBehaviorTelegramCommand<TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query, TSessionObject sessionObject);
        
        Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand, TQuery query, TSessionObject sessionObject);

        Task<ITelegramCommandExecutionResult> Execute<TQuery>(
            ISessionTelegramCommand<TQuery, TSessionObject> currentCommand, TQuery query, TSessionObject sessionObject);

    }
}