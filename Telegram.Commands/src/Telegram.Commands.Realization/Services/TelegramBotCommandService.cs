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
    internal sealed class TelegramBotCommandService : CommandServiceBase, IBotCommandResolver<Update, TelegramResult>
    {
        private readonly ITelegramBotClient _telegramClient;

        public TelegramBotCommandService(ITelegramBotClient telegramClient,
            IAuthProvider authProvider,
            SessionManager sessionManager,
            ITelegramBotProfile telegramBotProfile,
            CommandExecutionService commandExecutionService): base(commandExecutionService,sessionManager, authProvider, telegramBotProfile)
        {
            _telegramClient = telegramClient;
        }

        public async Task<TelegramResult> Handle(Update update)
        {
            long? chatId = null;
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        var telegramMessageQueryContext = new TelegramMessageQueryContext(update.Message);
                        chatId = telegramMessageQueryContext.GetChatId();
                        await QueryHandler(telegramMessageQueryContext);
                        break;
                    case UpdateType.CallbackQuery:
                        var callbackQueryContext = new CallbackQueryContext(update.CallbackQuery);
                        chatId = callbackQueryContext.GetChatId();
                        await QueryHandler(callbackQueryContext);
                        break;
                    case UpdateType.PreCheckoutQuery:
                        var preCheckoutQueryContext = new PreCheckoutQueryContext(update.PreCheckoutQuery);
                        chatId = preCheckoutQueryContext.GetChatId();
                        await QueryHandler(preCheckoutQueryContext);
                        break;
                    case UpdateType.MyChatMember:
                        var chatMemberUpdateQueryContext = new ChatMemberUpdateQueryContext(update.MyChatMember);
                        chatId = chatMemberUpdateQueryContext.GetChatId();
                        await QueryHandler(chatMemberUpdateQueryContext);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new TelegramResult(true);
            }
            catch (NotImplementedException ex)
            {
                if (chatId.HasValue)
                    await _telegramClient.SendTextMessageAsync(chatId, ex.Message);
                return new TelegramResult(false);
            }
        }
        
        protected override async Task SendDomainException<TQuery>(QueryContext<TQuery> query, TelegramDomainException ex)
        {
            await _telegramClient.SendTextMessageAsync(query.GetChatId(), ex.Message,
                replyToMessageId: query.GetMessageId());
        }
        
    }
}