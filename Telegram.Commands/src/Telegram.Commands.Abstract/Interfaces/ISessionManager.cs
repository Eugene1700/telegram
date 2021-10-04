using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionManager
    {
        ISessionInfo GetCurrentSession(long chatId, long telegramUserId);

        ISessionInfo GetSession<TCommand, TQuery>(long chatId, long telegramUserId)
            where TCommand : ITelegramCommand<TQuery>;

        Task ReleaseSessionIfExists(long chatId, long telegramUserId);

        Task<ISessionInfo> OpenSession<TCommand, TQuery, TData>(long chatId,
            long telegramUserId, TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : ITelegramCommand<TQuery>;
    }
}