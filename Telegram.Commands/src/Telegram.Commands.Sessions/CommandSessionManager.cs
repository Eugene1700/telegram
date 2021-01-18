using System;
using System.Linq;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;

namespace Telegram.Commands.Sessions
{
    public class CommandSessionManager : ICommandSessionManager
    {
        public SessionInfo OpenSession<T, TEntity, TQuery>(long chatId, TEntity[] entities) where T : ITelegramCommand<TQuery>
        {
            var sessionInfo = CreateSessionInfo<T, TQuery>(chatId);
            var sessionObj = CreateSessionData(sessionInfo, entities);
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
        
        private static SessionData<TEntity> CreateSessionData<TEntity>(SessionInfo sessionInfo, TEntity[] entities)
        {
            return new SessionData<TEntity>
            {
                SessionId = sessionInfo.Id,
                StoreObjects = entities.Select(x=> new StoreObject<TEntity>
                {
                    Id = Guid.NewGuid(),
                    Data = x
                }).ToArray()
            };
        }

        private static SessionInfo CreateSessionInfo<TCommand, TQuery>(long chatId) where TCommand : ITelegramCommand<TQuery>
        {
            return new SessionInfo
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                //todo implement IClockProvider
                OpenedAt = DateTimeOffset.Now,
                ReleasedAt = null,
                CommandId = TelegramCommandExtensions.GetCommandInfo<TCommand, TQuery>().Name,
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