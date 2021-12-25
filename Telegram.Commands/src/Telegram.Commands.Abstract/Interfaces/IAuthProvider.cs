using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IAuthProvider
    {
        Task<ITelegramUser> AuthUser(long userId, ITelegramCommandDescriptor commandDescriptor);
    }
}