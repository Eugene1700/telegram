using Bot.Command.Max.Models;
using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Bot.Command.Max.Services;

public class MaxCommandService(
    CommandExecutionService commandExecutionService,
    SessionManager sessionManager,
    IAuthProvider authProvider,
    IBotProfile botProfile)
    : CommandServiceBase(commandExecutionService,
        sessionManager, authProvider, botProfile), IBotCommandResolver<Update>
{
    public async Task Handle(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.MessageCreated:
                var telegramMessageQueryContext = new MessageCreatedQueryContext(update.Chat, update.Message);
                await QueryHandler(telegramMessageQueryContext);
                break;
            case UpdateType.MessageCallback:
                var callbackQueryContext = new MessageCallbackContext(update.Chat, update.Callback);
                await QueryHandler(callbackQueryContext);
                break;
            case UpdateType.UserAdded:
                var chatMemberUpdateQueryContext = new UserAddedUpdateContext(update.UserAddedUpdate);
                await QueryHandler(chatMemberUpdateQueryContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}