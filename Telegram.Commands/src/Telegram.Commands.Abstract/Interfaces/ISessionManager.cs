using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionManager
    {
        ISessionInfo GetCurrentSession(long chatId, long telegramUserId);

        CommandSession GetSession<TCommand, TQuery>(long chatId, long telegramUserId)
            where TCommand : ITelegramCommand<TQuery>;

        Task ReleaseSessionIfExists(long chatId, long telegramUserId);
    }
}