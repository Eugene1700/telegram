using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Abstract.Interfaces
{
    /// <summary>
    /// Interface to access to session manager from user's code
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Get current session for <see cref="ISessionTelegramCommand{TQuery,TSessionObject}"/>
        /// </summary>
        /// <param name="chatId">chat, where are doing conversation</param>
        /// <param name="telegramUserId">telegram user id, which initiate conversation</param>
        /// <typeparam name="TCommand">Type of session command</typeparam>
        /// <typeparam name="TQuery">Type of user's request</typeparam>
        /// <typeparam name="TData">Type of user's data</typeparam>
        /// <returns>session data <see cref="ISessionInfoWithData"/></returns>
        ISessionInfoWithData<TData> GetSession<TCommand, TQuery, TData>(long chatId, long telegramUserId)
            where TCommand : ISessionTelegramCommand<TQuery, TData>;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="telegramUserId"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        ISessionInfoWithData<TData> GetSession<TCommand, TData>(long chatId, long telegramUserId)
            where TCommand : IBehaviorTelegramCommand<TData>;

        Task ReleaseSessionIfExists(long chatId, long telegramUserId);

        Task<ISessionInfoWithData> OpenSession<TCommand, TQuery, TData>(long chatId,
            long telegramUserId, TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : ISessionTelegramCommand<TQuery, TData>;
    }
}