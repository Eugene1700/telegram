using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Services;
using Telegram.Commands.Realization.Models;

namespace Telegram.Commands.Realization.Services
{
    internal sealed class TelegramBotCommandService(
        IAuthProvider authProvider,
        SessionManager sessionManager,
        IBotProfile botProfile,
        CommandExecutionService commandExecutionService)
        : CommandServiceBase(commandExecutionService, sessionManager,
            authProvider, botProfile), IBotCommandResolver<Update>
    {
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
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}