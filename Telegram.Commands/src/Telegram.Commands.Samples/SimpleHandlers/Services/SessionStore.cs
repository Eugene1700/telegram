using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EntityStorage;
using EntityStorage.Extensions;
using SimpleHandlers.Domain;
using Bot.Commands.Abstract.Interfaces;
using IClock = Bot.Commands.Abstract.Interfaces.IClock;

namespace SimpleHandlers.Services
{
    public class SessionStore : ISessionsStore
    {
        private readonly IEntityStorage _entityStorage;
        private readonly BotPlatform _platform;
        private readonly IClock _clock;

        public SessionStore(IEntityStorage entityStorage, IClock clock, BotRuntime runtime)
        {
            _entityStorage = entityStorage;
            _platform = runtime.Platform;
            _clock = clock;
        }

        public ISessionInfo GetSessionInfo(DateTime now, long chatId, long userId)
        {
            var s = GetCurrentDomainSession(now, chatId, userId);
            return s == null
                ? null
                : new CourseSessionDescriptorBase
                {
                    ChatId = s.ChatId,
                    UserId = userId,
                    Data = null,
                    CommandQuery = s.CommandQuery,
                    ExpiredAt = s.ExpiredAt,
                    OpenedAt = s.OpenedAt
                };
        }

        public ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId, long userId, string commandName,
            Type sessionObject)
        {
            var s = GetCurrentDomainSession(now, chatId, userId);
            return s == null || s.CommandQuery != commandName
                ? null
                : new CourseSessionDescriptorBase
                {
                    ChatId = s.ChatId,
                    UserId = userId,
                    Data = JsonSerializer.Deserialize(s.SessionData, sessionObject),
                    CommandQuery = s.CommandQuery,
                    ExpiredAt = s.ExpiredAt,
                    OpenedAt = s.OpenedAt
                };
        }
        
        public ISessionInfoWithData<TData> GetSessionInfoWithData<TData>(DateTime now, long chatId, long userId, string commandName)
        {
            var s = GetCurrentDomainSession(now, chatId, userId);
            return s == null || s.CommandQuery != commandName
                ? null
                : new CourseSessionDescriptor<TData>
                {
                    ChatId = s.ChatId,
                    UserId = userId,
                    Data = JsonSerializer.Deserialize<TData>(s.SessionData),
                    CommandQuery = s.CommandQuery,
                    ExpiredAt = s.ExpiredAt,
                    OpenedAt = s.OpenedAt
                };
        }

        public async Task CreateSession(ISessionInfoWithData getCommandQuery)
        {
            var comSes = new CommandSession
            {
                Platform = _platform,
                CommandQuery = getCommandQuery.CommandQuery,
                SessionData = JsonSerializer.Serialize(getCommandQuery.Data),
                ExpiredAt = getCommandQuery.ExpiredAt,
                OpenedAt = getCommandQuery.OpenedAt,
                UserId = getCommandQuery.UserId,
                ChatId = getCommandQuery.ChatId
            };
            await _entityStorage.CreateEntity(comSes);
        }

        public async Task UpdateSession(ISessionInfoWithData session, long chatIdFrom)
        {
            var now = _clock.Now;
            var currentSession = GetCurrentDomainSession(now, chatIdFrom, session.UserId)
                ?? throw new InvalidOperationException("The session no longer exists on this platform.");
            await _entityStorage.UpdateSingle(currentSession, _ => new CommandSession
            {
                ChatId = session.ChatId,
                CommandQuery = session.CommandQuery,
                ExpiredAt = session.ExpiredAt,
                SessionData = JsonSerializer.Serialize(session.Data)
            });
        }

        private CommandSession GetCurrentDomainSession(DateTime now, long chatId, long userId)
        {
            return (from session in _entityStorage.Select<CommandSession>()
                where session.Platform == _platform
                where session.UserId == userId
                where session.ChatId == chatId
                where session.ExpiredAt == null || session.ExpiredAt > now
                select session).SingleOrDefault();
        }
    }

    public class CourseSessionDescriptorBase : ISessionInfoWithData
    {
        public string CommandQuery { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public object Data { get; set; }
    }

    public class CourseSessionDescriptor<TData> : CourseSessionDescriptorBase, ISessionInfoWithData<TData>
    {
        public new TData Data
        {
            get => base.Data == null ? default : (TData)base.Data;
            set => base.Data = value;
        }
    }
}
