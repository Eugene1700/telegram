using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class AuthProvider : IAuthProvider
    {
        public Task<bool> AuthUser(long telegramUserId, ITelegramCommandDescriptor commandDescriptor)
        {
            return Task.FromResult(true);
        }
    }
}