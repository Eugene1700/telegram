using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;

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
            var commandSession = new CommandSession()
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

        public async Task<CommandSession> ContinueSession(
            ITelegramCommandDescriptor nextCommandDescriptor, long chatId,
            long telegramUserId)
        {
            var session = GetCurrentSession(chatId, telegramUserId) as CommandSession;
            if (!SessionIsNotExpired(session) || session == null)
                throw new TelegramException("Session has been released");

            session.CommandQuery = nextCommandDescriptor.GetCommandQuery();
            session.ExpiredAt = session.ExpiredAt.AddMinutes(10);
            await _sessionsStore.UpdateSession(session);
            return session;
        }

        public CommandSession GetSession<TCommand, TQuery>(long chatId, long telegramUserId)
            where TCommand : ITelegramCommand<TQuery>
        {
            var session = (CommandSession) GetCurrentSession(chatId, telegramUserId);
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TCommand, TQuery>();
            if (TelegramQueryExtensions.ExtractCommand(session.CommandQuery) != commandInfo.Name)
                throw new TelegramException("Session is not consist");
            return session;
        }

        private async Task ReleaseSessionInternal(CommandSession currentSession)
        {
            currentSession.ExpiredAt = _clock.Now;
            await _sessionsStore.UpdateSession(currentSession);
        }

        public async Task ReleaseSessionIfExists(long chatId, long telegramUserId)
        {
            var currentSession = GetCurrentSession(chatId, telegramUserId);
            if (currentSession != null)
                await ReleaseSessionInternal(currentSession as CommandSession);
        }
    }
}