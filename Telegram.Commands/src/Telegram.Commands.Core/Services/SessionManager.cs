using System.Threading.Tasks;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Services
{
    public class SessionManager
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

        public async Task<CommandSession<TData>> OpenSession<TNextCommand, TQuery, TData>(long chatId, long telegramUserId, TData data,
            int sessionTimeInMinutes = 10)
            where TNextCommand : ITelegramCommand<TQuery>
        {
            var now = _clock.Now;
            if (GetCurrentSession(chatId, telegramUserId) != null)
                throw new TelegramException("Нельзя открывать новую сессию пока есть старая");
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery>();
            var commandSession = new CommandSession<TData>()
            {
                CommandQuery = commandInfo.GetCommandQuery(),
                OpenedAt = now,
                TelegramChatId = chatId,
                TelegramUserId = telegramUserId,
                ExpiredAt = now.AddMinutes(sessionTimeInMinutes),
                Data = data
            };
            await _sessionsStore.CreateSession(commandSession);
            return commandSession;
        }

        public async Task<CommandSession<TData>> ContinueSession<TNextCommand, TQuery, TData>(long chatId,
            long telegramUserId)
            where TNextCommand : ITelegramCommand<TQuery>
        {
            var session = GetCurrentSession(chatId, telegramUserId) as CommandSession<TData>;
            if (!SessionIsNotExpired(session) || session == null)
                throw new TelegramException("Session has been released");

            var commandDescriptor = TelegramCommandExtensions.GetCommandInfo<TNextCommand, TQuery>();
            session.CommandQuery = commandDescriptor.GetCommandQuery();
            session.ExpiredAt = session.ExpiredAt.AddMinutes(10);
            await _sessionsStore.UpdateSession(session);
            return session;
        }

        public CommandSession<TData> GetSession<TCommand, TQuery, TData>(int chatId, long telegramUserId)
            where TCommand : ITelegramCommand<TQuery>
        {
            var session = (CommandSession<TData>) GetCurrentSession(chatId, telegramUserId);
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TCommand, TQuery>();
            if (TelegramQueryExtensions.ExtractCommand(session.CommandQuery) != commandInfo.Name)
                throw new TelegramException("Session is not consist");
            return session;
        }

        private async Task ReleaseSessionInternal<TData>(CommandSession<TData> currentSession)
        {
            currentSession.ExpiredAt = _clock.Now;
            await _sessionsStore.UpdateSession(currentSession);
        }

        public async Task ReleaseSessionIfExists<TData>(long chatId, long telegramUserId)
        {
            var currentSession = GetCurrentSession(chatId, telegramUserId);
            if (currentSession != null)
                await ReleaseSessionInternal(currentSession as CommandSession<TData>);
        }
    }
}