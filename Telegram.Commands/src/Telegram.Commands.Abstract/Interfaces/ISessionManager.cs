using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Commands;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionManager
    {
        ISessionInfo GetCurrentSession(long chatId, long telegramUserId);
        ISessionInfoWithData<TData> GetSession<TCommand, TQuery, TData>(long chatId, long telegramUserId)
            where TCommand : ISessionTelegramCommand<TQuery, TData>;

        ISessionInfoWithData<TData> GetSession<TCommand, TData>(long chatId, long telegramUserId)
            where TCommand : IBehaviorTelegramCommand<TData>;

        Task ReleaseSessionIfExists(long chatId, long telegramUserId);

        Task<ISessionInfoWithData> OpenSession<TCommand, TQuery, TData>(long chatId,
            long telegramUserId, TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : ISessionTelegramCommand<TQuery, TData>;

        Task<ISessionInfoWithData> OpenBehaviorSession<TCommand, TData>(long chatId, long telegramUserId,
            TData sessionData, uint? sessionTimeInSec) where TCommand : IBehaviorTelegramCommand<TData>;
    }
}