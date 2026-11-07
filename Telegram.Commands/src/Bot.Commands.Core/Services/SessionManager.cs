using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Services
{
    public sealed class SessionManager : ISessionManager
    {
        private readonly IClock _clock;
        private readonly ISessionsStore _sessionsStore;

        public SessionManager(IClock clock, ISessionsStore sessionsStore)
        {
            _clock = clock;
            _sessionsStore = sessionsStore;
        }

        public ISessionInfo GetCurrentSession(long chatId, long userId)
        {
            var now = _clock.Now;
            return _sessionsStore.GetSessionInfo(now, chatId, userId);
        }
        
        public ISessionInfoWithData GetCurrentSession(long chatId, long userId, string commandQuery, Type sessionObjectType)
        {
            var now = _clock.Now;
            return _sessionsStore.GetSessionInfoWithData(now, chatId, userId, commandQuery, sessionObjectType);
        }

        private bool SessionIsNotExpired(ISessionInfo s)
        {
            var now = _clock.Now;
            return !s.ExpiredAt.HasValue || s.ExpiredAt > now;
        }

        public async Task<CommandSession> OpenSession(ICommandDescriptor nextCommandDescriptor, long chatId,
            long userId, object sessionData,
            uint? sessionTimeInSec = 600)
        {
            var now = _clock.Now;
            if (GetCurrentSession(chatId, userId) != null)
                throw new CommandsInternalException("Session has been already opened");
            var commandSession = new CommandSession
            {
                CommandQuery = nextCommandDescriptor.GetCommandQuery(),
                OpenedAt = now,
                ChatId = chatId,
                UserId = userId,
                ExpiredAt = sessionTimeInSec.HasValue ? 
                    now.AddSeconds(sessionTimeInSec.Value) : (DateTime?) null,
                Data = sessionData
            };
            await _sessionsStore.CreateSession(commandSession);
            return commandSession;
        }

        public async Task<ISessionInfoWithData> ContinueSession(
            ICommandDescriptor nextCommandDescriptor, long chatIdFrom, long chatIdTo,
            long userId, object sessionData,
            uint? sessionTimeInSec = 600)
        {
            var session = GetCurrentSession(chatIdFrom, userId);
            if (session == null || !SessionIsNotExpired(session))
                throw new CommandsInternalException("Session has been released");

            if (chatIdFrom != chatIdTo)
            {
                var sessionToChatSession
                    = GetCurrentSession(chatIdTo, userId);
                if (sessionToChatSession != null &&
                    sessionToChatSession.CommandQuery == nextCommandDescriptor.GetCommandQuery())
                    throw new CommandsInternalException("You must complete session in next chat");
            }

            var expiredTime = session.ExpiredAt;
            if (sessionTimeInSec.HasValue)
            {
                expiredTime = expiredTime?.AddSeconds(sessionTimeInSec.Value) 
                              ?? _clock.Now.AddSeconds(sessionTimeInSec.Value);
            }
            else
            {
                expiredTime = null;
            }
            var ses = CreateCommandSession(session, expiredTime,
                nextCommandDescriptor.GetCommandQuery(), chatIdTo, sessionData);
            
            await _sessionsStore.UpdateSession(ses, chatIdFrom);
            return ses;
        }

        public ISessionInfoWithData<TData> GetSession<TCommand, TQuery, TData>(long chatId, long userId)
            where TCommand : ISessionCommand<TQuery, TData>
        {
            var now = _clock.Now;
            var commandInfo = BotCommandExtensions.GetCommandInfo<TCommand, TQuery, TData>();
            var commandQuery = commandInfo.GetCommandQuery();
            var session = _sessionsStore.GetSessionInfoWithData<TData>(now, chatId, userId, commandQuery);
            if (session == null)
                return null;
            return commandQuery != session.CommandQuery ? null : session;
        }

        public ISessionInfoWithData<TData> GetSession<TCommand, TData>(long chatId, long userId) where TCommand : IBehaviorCommand<TData>
        {
            var now = _clock.Now;
            var commandInfo = BotCommandExtensions.GetBehaviorCommandInfo<TCommand, TData>();
            var commandQuery = commandInfo.GetCommandQuery();
            var session = _sessionsStore.GetSessionInfoWithData<TData>(now, chatId, userId, commandQuery);
            if (session == null)
                return null;
            return commandQuery != session.CommandQuery ? null : session;
        }

        private async Task ReleaseSessionInternal(ISessionInfo currentSession)
        {
            var commandSession = CreateCommandSession(currentSession, _clock.Now, currentSession.CommandQuery,
                currentSession.ChatId, null);
            await _sessionsStore.UpdateSession(commandSession, currentSession.ChatId);
        }

        private static CommandSession CreateCommandSession(ISessionInfo currentSession, DateTime? expiredAt,
            string commandQuery, long chatId, object sessionData)
        {
            return new CommandSession
            {
                CommandQuery = commandQuery,
                OpenedAt = currentSession.OpenedAt,
                ChatId = chatId,
                UserId = currentSession.UserId,
                ExpiredAt = expiredAt,
                Data = sessionData
            };
        }

        public async Task ReleaseSessionIfExists(long chatId, long userId)
        {
            var currentSession = GetCurrentSession(chatId, userId);
            if (currentSession != null)
            {
                await ReleaseSessionInternal(currentSession);
            }
        }

        public async Task<ISessionInfoWithData> OpenSession<TCommand, TQuery, TData>(long chatId, long userId, TData sessionData, uint? sessionTimeInSec) where TCommand : ISessionCommand<TQuery, TData>
        {
            return await OpenSession(BotCommandExtensions.GetCommandInfo<TCommand, TQuery, TData>(), chatId, userId,
                sessionData, sessionTimeInSec);
        }

        public async Task<ISessionInfoWithData> OpenBehaviorSession<TCommand, TData>(long chatId, long userId, TData sessionData, uint? sessionTimeInSec) where TCommand : IBehaviorCommand<TData>
        {
            return await OpenSession(BotCommandExtensions.GetBehaviorCommandInfo<TCommand, TData>(), chatId, userId,
                sessionData, sessionTimeInSec);
        }
    }
}
