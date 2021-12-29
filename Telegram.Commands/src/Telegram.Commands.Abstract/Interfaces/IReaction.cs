using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IBehaviorCommand<TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> Execute<TQuery>(ITelegramCommand<TQuery> currentCommand, TQuery query);

        Task<ITelegramCommandExecutionResult> Execute<TQuery>(
            ISessionTelegramCommand<TQuery, TSessionObject> currentCommand, TQuery query, TSessionObject sessionObject);

    }
}