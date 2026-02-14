using Bot.Command.Max.Models;
using Bot.Command.Max.Services;
using Max.Bot;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Services;

namespace Bot.Command.Max;

public class MaxCommandService : CommandServiceBase, IBotCommandResolver<Update, MaxResult>
{
    private readonly MaxClient _maxClient;

    public MaxCommandService(MaxClient maxClient, CommandExecutionService commandExecutionService, SessionManager sessionManager,
        IAuthProvider authProvider, IBotProfile botProfile) : base(commandExecutionService,
        sessionManager, authProvider, botProfile)
    {
        _maxClient = maxClient;
    }

    public async Task<MaxResult> Handle(Update update)
    {
        long? chatId = null;
        try
        {
            switch (update.Type)
            {
                case UpdateType.MessageCreated:
                    var telegramMessageQueryContext = new MessageCreatedQueryContext(update.Chat, update.Message);
                    chatId = telegramMessageQueryContext.GetChatId();
                    await QueryHandler(telegramMessageQueryContext);
                    break;
                case UpdateType.MessageCallback:
                    var callbackQueryContext = new MessageCallbackContext(update.Chat, update.Callback);
                    chatId = callbackQueryContext.GetChatId();
                    await QueryHandler(callbackQueryContext);
                    break;
                case UpdateType.UserAdded:
                    var chatMemberUpdateQueryContext = new ChatMemberUpdateQueryContext(update.UserAddedUpdate);
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
                await _maxClient.Messages.SendMessageAsync(chatId.Value, ex.Message);
            return new MaxResult();
        }
    }

    protected override Task SendDomainException<TQuery>(QueryContext<TQuery> query, TelegramDomainException ex)
    {
        throw new NotImplementedException();
    }
}