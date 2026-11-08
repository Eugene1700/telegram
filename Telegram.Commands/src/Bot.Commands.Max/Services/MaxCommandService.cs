using Bot.Commands.Max.Models;
using global::Max.Bot.Types;
using global::Max.Bot.Types.Enums;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Max.Services;

public class MaxCommandService(
    CommandExecutionService commandExecutionService,
    SessionManager sessionManager,
    IAuthProvider authProvider,
    IBotProfile botProfile,
    IEnumerable<ICommandMatcher<Message>>? messageMatchers = null)
    : CommandServiceBase(commandExecutionService,
        sessionManager, authProvider, botProfile), IBotCommandResolver<Update>
{
    public async Task Handle(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.MessageCreated:
                var messageQueryContext = new MessageCreatedQueryContext(update.Chat, update.Message);
                await QueryHandler(messageQueryContext, messageMatchers);
                break;
            case UpdateType.MessageCallback:
                var callbackQueryContext = new MessageCallbackContext(update.Chat, update.Callback, update.Message);
                await QueryHandler(callbackQueryContext);
                break;
            case UpdateType.UserAdded:
                var chatMemberUpdateQueryContext = new UserAddedUpdateContext(update.UserAddedUpdate, update.ChatId);
                await QueryHandler(chatMemberUpdateQueryContext);
                break;
            default:
                break;
        }
    }
}
