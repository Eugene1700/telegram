namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionManager
    {
        ISessionInfo GetCurrentSession(long chatId, long userId);
    }
}