using System;
using System.Linq;
using System.Threading.Tasks;
using EntityStorage;
using Newtonsoft.Json;
using SimpleHandlers.Domain;
using Telegram.Commands.Abstract.Interfaces;
using IClock = Telegram.Commands.Abstract.Interfaces.IClock;

namespace SimpleHandlers.Services
{
    public class SessionStore : ISessionsStore
    {
        private readonly IEntityStorage _entityStorage;
        private readonly IClock _clock;

        public SessionStore(IEntityStorage entityStorage, IClock clock)
        {
            _entityStorage = entityStorage;
            _clock = clock;
        }

        public ISessionInfo GetSessionInfo(DateTime now, long chatId, long telegramUserId)
        {
            var s = GetCurrentDomainSession(now, chatId, telegramUserId);
            return s == null
                ? null
                : new CourseSessionDescriptorBase
                {
                    TelegramChatId = s.TelegramChatId,
                    TelegramUserId = telegramUserId,
                    Data = null,
                    CommandQuery = s.CommandQuery,
                    ExpiredAt = s.ExpiredAt,
                    OpenedAt = s.OpenedAt
                };
        }

        public ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId, long telegramUserId,
            Type sessionObject)
        {
            var s = GetCurrentDomainSession(now, chatId, telegramUserId);
            return s == null
                ? null
                : new CourseSessionDescriptorBase
                {
                    TelegramChatId = s.TelegramChatId,
                    TelegramUserId = telegramUserId,
                    Data = JsonConvert.DeserializeObject(s.SessionData, sessionObject),
                    CommandQuery = s.CommandQuery,
                    ExpiredAt = s.ExpiredAt,
                    OpenedAt = s.OpenedAt
                };
        }
        
        public ISessionInfoWithData<TData> GetSessionInfoWithData<TData>(DateTime now, long chatId, long telegramUserId)
        {
            var s = GetCurrentDomainSession(now, chatId, telegramUserId);
            return s == null
                ? null
                : new CourseSessionDescriptor<TData>
                {
                    TelegramChatId = s.TelegramChatId,
                    TelegramUserId = telegramUserId,
                    Data = JsonConvert.DeserializeObject<TData>(s.SessionData),
                    CommandQuery = s.CommandQuery,
                    ExpiredAt = s.ExpiredAt,
                    OpenedAt = s.OpenedAt
                };
        }

        public async Task CreateSession(ISessionInfoWithData getCommandQuery)
        {
            var comSes = new CommandSession
            {
                CommandQuery = getCommandQuery.CommandQuery,
                SessionData = JsonConvert.SerializeObject(getCommandQuery.Data),
                ExpiredAt = getCommandQuery.ExpiredAt,
                OpenedAt = getCommandQuery.OpenedAt,
                TelegramUserId = getCommandQuery.TelegramUserId,
                TelegramChatId = getCommandQuery.TelegramChatId
            };
            await _entityStorage.CreateEntity(comSes);
        }

        public async Task UpdateSession(ISessionInfoWithData session, long chatIdFrom)
        {
            var now = _clock.Now;
            var currentSession = GetCurrentDomainSession(now, chatIdFrom, session.TelegramUserId);
            await _entityStorage.UpdateSingle(currentSession, _ => new CommandSession
            {
                TelegramChatId = session.TelegramChatId,
                CommandQuery = session.CommandQuery,
                ExpiredAt = session.ExpiredAt,
                SessionData = JsonConvert.SerializeObject(session.Data)
            });
        }

        private CommandSession GetCurrentDomainSession(DateTime now, long chatId, long telegramUserId)
        {
            return (from session in _entityStorage.Select<CommandSession>()
                where session.TelegramUserId == telegramUserId
                where session.TelegramChatId == chatId
                where session.ExpiredAt == null || session.ExpiredAt > now
                select session).SingleOrDefault();
        }
    }

    public class CourseSessionDescriptorBase : ISessionInfoWithData
    {
        public string CommandQuery { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public long TelegramChatId { get; set; }
        public long TelegramUserId { get; set; }
        public object Data { get; set; }
    }

    public class CourseSessionDescriptor<TData> : CourseSessionDescriptorBase, ISessionInfoWithData<TData>
    {
        public new TData Data { get; set; }
    }
}