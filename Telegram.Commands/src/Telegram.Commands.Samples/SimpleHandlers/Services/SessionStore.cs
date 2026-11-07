using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EntityStorage;
using EntityStorage.Extensions;
using SimpleHandlers.Domain;
using Bot.Commands.Abstract.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using IClock = Bot.Commands.Abstract.Interfaces.IClock;

namespace SimpleHandlers.Services
{
    public class SessionStore : ISessionsStore
    {
        private readonly IEntityStorage _entityStorage;
        private readonly string _botName;
        private readonly IClock _clock;
        private readonly ILogger<SessionStore> _logger;

        public SessionStore(IEntityStorage entityStorage, IClock clock, IBotProfile profile,
            ILogger<SessionStore> logger = null)
        {
            _entityStorage = entityStorage;
            _botName = !string.IsNullOrWhiteSpace(profile.BotName) ? profile.BotName
                : throw new ArgumentException("BotName is required for session isolation.", nameof(profile));
            _clock = clock;
            _logger = logger ?? NullLogger<SessionStore>.Instance;
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
                BotName = _botName,
                CommandQuery = getCommandQuery.CommandQuery,
                SessionData = JsonSerializer.Serialize(getCommandQuery.Data),
                ExpiredAt = getCommandQuery.ExpiredAt,
                OpenedAt = getCommandQuery.OpenedAt,
                UserId = getCommandQuery.UserId,
                ChatId = getCommandQuery.ChatId
            };
            await _entityStorage.CreateEntity(comSes);
            _logger.LogInformation("Created session for bot {BotName}, command {Command}, chat {ChatId}, user {UserId}",
                _botName, comSes.CommandQuery, comSes.ChatId, comSes.UserId);
        }

        public async Task UpdateSession(ISessionInfoWithData session, long chatIdFrom)
        {
            var now = _clock.Now;
            var currentSession = GetCurrentDomainSession(now, chatIdFrom, session.UserId)
                ?? throw new InvalidOperationException("The session no longer exists for this bot.");
            await _entityStorage.UpdateSingle(currentSession, _ => new CommandSession
            {
                BotName = _botName,
                UserId = currentSession.UserId,
                OpenedAt = currentSession.OpenedAt,
                ChatId = session.ChatId,
                CommandQuery = session.CommandQuery,
                ExpiredAt = session.ExpiredAt,
                SessionData = JsonSerializer.Serialize(session.Data)
            });
            _logger.LogInformation("Updated session for bot {BotName}, command {Command}, chat {ChatId}, user {UserId}",
                _botName, session.CommandQuery, session.ChatId, session.UserId);
        }

        private CommandSession GetCurrentDomainSession(DateTime now, long chatId, long userId)
        {
            return (from session in _entityStorage.Select<CommandSession>()
                where session.BotName == _botName
                where session.UserId == userId
                where session.ChatId == chatId
                where session.ExpiredAt == null || session.ExpiredAt > now
                select session).SingleOrDefault();
        }
    }
}
