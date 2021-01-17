using System;
using System.Diagnostics.Contracts;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Sessions
{
    public interface ISessionManager
    {
        Session<TEntity> StoreSession<TNextCommand, TEntity, TQuery>(long chatId, TEntity[] entities) where TNextCommand : ITelegramCommand<TQuery>;
        Session<TEntity> StoreSession<TNextCommand, TEntity, TQuery>(long chatId, TEntity entity) where TNextCommand : ITelegramCommand<TQuery>;
        StoreObject<TEntity>[] GetData<TCommand, TEntity, TQuery>(long chatId) where TCommand : ITelegramCommand<TQuery>;
        void ReleaseSession<TCommand, TEntity, TQuery>(long chatId) where TCommand : ITelegramCommand<TQuery>;
        bool TryGetData<TCommand, TEntity, TQuery>(long chatId, Guid id, out TEntity res) where TCommand : ITelegramCommand<TQuery>;
    }
    
    public class StoreObject<T>
    {
        public Guid Id { get; set; }
        public T Data { get; set; }
    }

    public class Session<T>
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public string CommandId { get; set; }
        public DateTimeOffset OpenedAt { get; set; }
        public DateTimeOffset? ReleasedAt { get; set; }
        public StoreObject<T>[] StoreObjects { get; set; }
    }
}