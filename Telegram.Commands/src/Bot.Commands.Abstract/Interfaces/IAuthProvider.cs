using System.Threading.Tasks;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface IAuthProvider
    {
        Task<bool> AuthUser<TQuery>(long userId, ICommandExecutionContext<TQuery> context);
    }
}