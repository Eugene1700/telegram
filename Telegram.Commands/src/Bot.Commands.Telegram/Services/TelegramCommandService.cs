using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using Bot.Commands.Abstract;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Services;
using Bot.Commands.Telegram.Models;

namespace Bot.Commands.Telegram.Services
{
    public sealed class TelegramCommandService(
        IAuthProvider authProvider,
        SessionManager sessionManager,
        IBotProfile botProfile,
        CommandExecutionService commandExecutionService)
        : CommandServiceBase(commandExecutionService, sessionManager,
            authProvider, botProfile), IBotCommandResolver<Update>
    {
        protected override bool TryGetQueryCommandStr<TQuery>(QueryContext<TQuery> query, out string commandStr)
        {
            if (!base.TryGetQueryCommandStr(query, out commandStr)) return false;
            if (query is not QueryContext<Message>) return true;

            var token = commandStr.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[0];
            var mention = token.IndexOf('@');
            if (mention < 0) return !query.IsGroupMessage();
            if (!token[(mention + 1)..].Equals(BotProfile.BotName, StringComparison.OrdinalIgnoreCase))
                return false;
            commandStr = token[..mention];
            return true;
        }

        public async Task Handle(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    var telegramMessageQueryContext = new TelegramMessageQueryContext(update.Message);
                    await QueryHandler(telegramMessageQueryContext);
                    break;
                case UpdateType.CallbackQuery:
                    var callbackQueryContext = new CallbackQueryContext(update.CallbackQuery);
                    await QueryHandler(callbackQueryContext);
                    break;
                case UpdateType.PreCheckoutQuery:
                    var preCheckoutQueryContext = new PreCheckoutQueryContext(update.PreCheckoutQuery);
                    await QueryHandler(preCheckoutQueryContext);
                    break;
                case UpdateType.MyChatMember:
                    var chatMemberUpdateQueryContext = new ChatMemberUpdateQueryContext(update.MyChatMember);
                    await QueryHandler(chatMemberUpdateQueryContext);
                    break;
                default:
                    break;
            }
        }
    }
}
