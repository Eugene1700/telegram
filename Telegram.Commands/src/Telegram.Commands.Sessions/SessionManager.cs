using System;
using System.Linq;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;

namespace Telegram.Commands.Sessions
{
    public class SessionManager : ISessionManager
    {
        private readonly ISessionStore _store;

        public SessionManager(ISessionStore store)
        {
            _store = store;
        }
        
        public Session<TEntity> StoreSession<T, TEntity, TQuery>(long chatId, TEntity[] entities) where T : ITelegramCommand<TQuery>
        {
            var sessionObj = CreateSessionObject<T, TEntity, TQuery>(chatId, entities);
            _store.Save(sessionObj);
            return sessionObj;
        }

        public Session<TEntity> StoreSession<T, TEntity, TQuery>(long chatId, TEntity entity) where T : ITelegramCommand<TQuery>
        {
            return StoreSession<T, TEntity, TQuery>(chatId, new[] {entity});
        }

        public StoreObject<TEntity>[] GetData<T, TEntity, TQuery>(long chatId) where T : ITelegramCommand<TQuery>
        {
            var session = GetSession<T, TEntity, TQuery>(chatId);
            return session.StoreObjects;
        }

        public void ReleaseSession<T, TEntity, TQuery>(long chatId) where T : ITelegramCommand<TQuery>
        {
            var session = GetSession<T, TEntity, TQuery>(chatId);
            //todo implement IClockProvider
            session.ReleasedAt = DateTimeOffset.Now;
            _store.Update(session);
        }

        public bool TryGetData<T, TEntity, TQuery>(long chatId, Guid id, out TEntity res) where T : ITelegramCommand<TQuery>
        {
            var session = GetSession<T, TEntity, TQuery>(chatId);
            var sessionObj = session.StoreObjects.SingleOrDefault(x => x.Id == id);
            res = sessionObj == null ? default : sessionObj.Data;
            return sessionObj != null;
        }
        
        private static Session<TEntity> CreateSessionObject<T, TEntity, TQuery>(long chatId, TEntity[] entities) where T : ITelegramCommand<TQuery>
        {
            return new Session<TEntity>
            {
                ChatId = chatId,
                //todo implement IClockProvider
                OpenedAt = DateTimeOffset.Now,
                ReleasedAt = null,
                CommandId = TelegramCommandExtensions.GetCommandInfo<T, TQuery>().Name,
                StoreObjects = entities.Select(x=> new StoreObject<TEntity>
                {
                    Id = Guid.NewGuid(),
                    Data = x
                }).ToArray()
            };
        }
        
        private Session<TEntity> GetSession<T, TEntity, TQuery>(long chatId) where T : ITelegramCommand<TQuery>
        {
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<T, TQuery>();
            var session = _store.Get<TEntity>(chatId, commandInfo.Name);
            return session;
        }
    }
}