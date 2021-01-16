using System.Threading.Tasks;

namespace Telegram.Commands.Core
{
    public interface IAuthProvider
    {
        Task<ITelegramUser> AuthUser(int userId);
    }
}