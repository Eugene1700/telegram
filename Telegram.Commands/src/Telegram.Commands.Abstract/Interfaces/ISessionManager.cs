using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Abstract.Interfaces
{
    //todo
    /// <summary>
    /// Services 
    /// </summary>
    public interface ISessionManager
    {
        ISessionInfoWithData<TData> GetSession<TCommand, TQuery, TData>(long chatId, long telegramUserId)
            where TCommand : ISessionTelegramCommand<TQuery, TData>;
        
        ISessionInfoWithData<TData> GetSession<TCommand, TData>(long chatId, long telegramUserId)
            where TCommand : IBehaviorTelegramCommand<TData>;

        Task ReleaseSessionIfExists(long chatId, long telegramUserId);

        Task<ISessionInfoWithData> OpenSession<TCommand, TQuery, TData>(long chatId,
            long telegramUserId, TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : ISessionTelegramCommand<TQuery, TData>;
    }
}