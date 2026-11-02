using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface ISessionManager
    {
        ISessionInfo GetCurrentSession(long chatId, long userId);
        ISessionInfoWithData<TData> GetSession<TCommand, TQuery, TData>(long chatId, long userId)
            where TCommand : ISessionCommand<TQuery, TData>;

        ISessionInfoWithData<TData> GetSession<TCommand, TData>(long chatId, long userId)
            where TCommand : IBehaviorCommand<TData>;

        Task ReleaseSessionIfExists(long chatId, long userId);

        Task<ISessionInfoWithData> OpenSession<TCommand, TQuery, TData>(long chatId,
            long userId, TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : ISessionCommand<TQuery, TData>;

        Task<ISessionInfoWithData> OpenBehaviorSession<TCommand, TData>(long chatId, long userId,
            TData sessionData, uint? sessionTimeInSec) where TCommand : IBehaviorCommand<TData>;
    }
}