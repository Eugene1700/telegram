using System.Threading.Tasks;

namespace Bot.Commands.Abstract.Interfaces
{

    public abstract class ExecuteFilterAttribute
    {
        public abstract Task Filter<TQuery>(ICommandExecutionContext<TQuery> context);
    }
}