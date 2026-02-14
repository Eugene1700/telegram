using Bot.Command.Max.Services;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Services;

namespace Bot.Command.Max;

public class MaxCommandService: CommandServiceBase, IBotCommandResolver<Update, MaxResult>
{
    public MaxCommandService(CommandExecutionService commandExecutionService, SessionManager sessionManager, IAuthProvider authProvider, ITelegramBotProfile telegramBotProfile) : base(commandExecutionService, sessionManager, authProvider, telegramBotProfile)
    {
        
    }

    public async Task<MaxResult> Handle(Update update)
    {
        long? chatId = null;
        try
        {
            switch (update.Type)
            {
                case UpdateType.MessageCreated:
                    var telegramMessageQueryContext = new TelegramMessageQueryContext(update.Message);
                    chatId = telegramMessageQueryContext.GetChatId();
                    await QueryHandler(telegramMessageQueryContext);
                    break;
                case UpdateType.MessageCallback:
                    var callbackQueryContext = new CallbackQueryContext(update.Callback);
                    chatId = callbackQueryContext.GetChatId();
                    await QueryHandler(callbackQueryContext);
                    break;
                case UpdateType.UserAdded:
                    var chatMemberUpdateQueryContext = new ChatMemberUpdateQueryContext(update.MyChatMember);
                    chatId = chatMemberUpdateQueryContext.GetChatId();
                    await QueryHandler(chatMemberUpdateQueryContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new MaxResult();
        }
        catch (NotImplementedException ex)
        {
            if (chatId.HasValue)
                await _telegramClient.SendTextMessageAsync(chatId, ex.Message);
            return new TelegramResult(false);
        }
    }

    protected override Task SendDomainException<TQuery>(QueryContext<TQuery> query, TelegramDomainException ex)
    {
        throw new NotImplementedException();
    }
}