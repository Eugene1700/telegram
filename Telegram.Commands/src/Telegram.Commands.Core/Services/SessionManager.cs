using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Services
{
    internal sealed class SessionManager : ISessionManager
    {
        private readonly IClock _clock;
        private readonly ISessionsStore _sessionsStore;

        public SessionManager(IClock clock, ISessionsStore sessionsStore)
        {
            _clock = clock;
            _sessionsStore = sessionsStore;
        }

        public ISessionInfo GetCurrentSession(long chatId, long telegramUserId)
        {
            var now = _clock.Now;
            return _sessionsStore.GetSessionInfo(now, chatId, telegramUserId);
        }

        private bool SessionIsNotExpired(ISessionInfo s)
        {
            var now = _clock.Now;
            return s.ExpiredAt > now;
        }

        public async Task<CommandSession> OpenSession(ITelegramCommandDescriptor nextCommandDescriptor, long chatId,
            long telegramUserId, object sessionData,
            int sessionTimeInMinutes = 10)
        {
            var now = _clock.Now;
            if (GetCurrentSession(chatId, telegramUserId) != null)
                throw new TelegramException("Нельзя открывать новую сессию пока есть старая");
            var commandSession = new CommandSession
            {
                CommandQuery = nextCommandDescriptor.GetCommandQuery(),
                OpenedAt = now,
                TelegramChatId = chatId,
                TelegramUserId = telegramUserId,
                ExpiredAt = now.AddMinutes(sessionTimeInMinutes),
                Data = sessionData
            };
            await _sessionsStore.CreateSession(commandSession);
            return commandSession;
        }

        public async Task<ISessionInfo> ContinueSession(
            ITelegramCommandDescriptor nextCommandDescriptor, long chatIdFrom, long chatIdTo,
            long telegramUserId)
        {
            var session = GetCurrentSession(chatIdFrom, telegramUserId);
            if (!SessionIsNotExpired(session) || session == null)
                throw new TelegramException("Session has been released");

            var ses = CreateCommandSession(session, session.ExpiredAt.AddMinutes(10),
                nextCommandDescriptor.GetCommandQuery(), chatIdTo);
            await _sessionsStore.UpdateSession(ses, chatIdFrom);
            return ses;
        }

        public ISessionInfo GetSession<TCommand, TQuery>(long chatId, long telegramUserId)
            where TCommand : ITelegramCommand<TQuery>
        {
            var session = GetCurrentSession(chatId, telegramUserId);
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TCommand, TQuery>();
            if (TelegramQueryExtensions.ExtractCommand(session.CommandQuery) != commandInfo.Name)
                throw new TelegramException("Session is not consist");
            return session;
        }

        private async Task ReleaseSessionInternal(ISessionInfo currentSession)
        {
            var commandSession = CreateCommandSession(currentSession, _clock.Now, currentSession.CommandQuery,
                currentSession.TelegramChatId);
            await _sessionsStore.UpdateSession(commandSession, currentSession.TelegramChatId);
        }

        private CommandSession CreateCommandSession(ISessionInfo currentSession, DateTime expiredAt,
            string commandQuery, long chatId)
        {
            return new CommandSession
            {
                CommandQuery = commandQuery,
                OpenedAt = currentSession.OpenedAt,
                TelegramChatId = chatId,
                TelegramUserId = currentSession.TelegramUserId,
                ExpiredAt = expiredAt,
                Data = currentSession.Data
            };
        }

        public async Task ReleaseSessionIfExists(long chatId, long telegramUserId)
        {
            var currentSession = GetCurrentSession(chatId, telegramUserId);
            if (currentSession != null)
            {
                await ReleaseSessionInternal(currentSession);
            }
        }
    }
}