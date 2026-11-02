using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class AuthProvider : IAuthProvider
    {
        public Task<bool> AuthUser<TQuery>(long userId, ICommandExecutionContext<TQuery> context)
        {
            return Task.FromResult(true);
        }
    }
}