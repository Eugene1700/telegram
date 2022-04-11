using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class AuthProvider : IAuthProvider
    {
        public Task<bool> AuthUser<TQuery>(long telegramUserId, ICommandExecutionContext<TQuery> context)
        {
            return Task.FromResult(true);
        }
    }
}