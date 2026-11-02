using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Abstract.Commands
{
    public interface ISessionCommand<TQuery, in TSessionObject>
    {
        Task<ICommandExecutionResult> Execute(QueryContext<TQuery> query, TSessionObject sessionObject);
    }
}