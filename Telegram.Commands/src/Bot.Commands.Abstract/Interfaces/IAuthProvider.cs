using System.Threading.Tasks;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface IAuthProvider
    {
        Task<bool> AuthUser<TQuery>(long userId, ICommandExecutionContext<TQuery> context);
    }

    public abstract class ExecuteFilterAttribute
    {
        public abstract Task Filter<TQuery>(ICommandExecutionContext<TQuery> context);
    }
}