namespace Telegram.Commands.Core
{
    public interface ITelegramUser
    {
        public long UserId { get; set; }
        Permissions Permission { get; set; }
    }
}