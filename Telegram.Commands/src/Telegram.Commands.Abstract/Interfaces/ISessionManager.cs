using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionManager
    {
        ISessionInfoWithData GetCurrentSession(long chatId, long telegramUserId, Type sessionObjectType);

        ISessionInfo GetSession<TCommand, TQuery>(long chatId, long telegramUserId)
            where TCommand : ITelegramCommand<TQuery>;

        Task ReleaseSessionIfExists(long chatId, long telegramUserId);

        Task<ISessionInfoWithData> OpenSession<TCommand, TQuery, TData>(long chatId,
            long telegramUserId, TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : ISessionTelegramCommand<TQuery, TData>;
    }
}