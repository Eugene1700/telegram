using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Abstract.Commands
{
    public interface IQueryCommand<TQuery>
    {
        Task<ICommandExecutionResult> Execute(IQueryContext<TQuery> query);
    }
}