using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IAuthProvider
    {
        Task<bool> AuthUser(long telegramUserId, ITelegramCommandDescriptor commandDescriptor);
    }
}