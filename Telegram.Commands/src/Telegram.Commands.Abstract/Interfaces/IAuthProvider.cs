using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core
{
    public interface IAuthProvider
    {
        Task<ITelegramUser> AuthUser(int userId);
    }
}