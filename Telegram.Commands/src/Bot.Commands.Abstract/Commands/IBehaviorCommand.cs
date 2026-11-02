using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Abstract.Commands
{
    public interface IBehaviorCommand<TSessionObject>
    {
        Task<ICommandExecutionResult> DefaultExecute<TQuery>(QueryContext<TQuery> query, TSessionObject sessionObject);

        Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> currentCommand, QueryContext<TQuery> query, TSessionObject sessionObject);

        Task<ICommandExecutionResult> Execute<TQuery>(
            ISessionCommand<TQuery, TSessionObject> currentCommand, QueryContext<TQuery> query, TSessionObject sessionObject);

    }
}