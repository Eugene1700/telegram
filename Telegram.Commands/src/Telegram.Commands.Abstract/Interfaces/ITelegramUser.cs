using Telegram.Commands.Core;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramUser
    {
        public long UserId { get; set; }
        Permissions Permission { get; set; }
    }
}