using System;
using System.Diagnostics.Contracts;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Sessions
{
    public interface ICommandSessionManager
    {
        SessionInfo OpenSession<TNextCommand, TQuery>(long chatId) where TNextCommand : ITelegramCommand<TQuery>;
        // SessionInfo OpenSession<TNextCommand, TEntity, TQuery>(long chatId, TEntity entity) where TNextCommand : ITelegramCommand<TQuery>;
        // StoreObject<TEntity>[] GetData<TCommand, TEntity, TQuery>(long chatId) where TCommand : ITelegramCommand<TQuery>;
        void ReleaseSession(SessionInfo sessionInfo);
        // bool TryGetData<TCommand, TEntity, TQuery>(long chatId, Guid id, out TEntity res) where TCommand : ITelegramCommand<TQuery>;
        SessionInfo GetCurrentSession(long chatId);
    }
    
    public class StoreObject<T>
    {
        public Guid Id { get; set; }
        public T Data { get; set; }
    }

    public class SessionInfo
    {
        public Guid Id { get; set; }
        public long ChatId { get; set; }
        public string CommandId { get; set; }
        public DateTimeOffset OpenedAt { get; set; }
        public DateTimeOffset? ReleasedAt { get; set; }
    }

    public class SessionData<T>
    {
        public Guid SessionId { get; set; }
        public StoreObject<T>[] StoreObjects { get; set; }
    }
}