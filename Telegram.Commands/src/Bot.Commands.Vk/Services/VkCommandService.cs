using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Services;
using Bot.Commands.Vk.Models;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace Bot.Commands.Vk.Services;

public class VkCommandService(
    CommandExecutionService commandExecutionService,
    SessionManager sessionManager,
    IAuthProvider authProvider,
    IBotProfile botProfile,
    IEnumerable<ICommandMatcher<Message>>? messageMatchers = null,
    IEnumerable<ICommandMatcher<MessageEvent>>? callbackMatchers = null)
    : CommandServiceBase(commandExecutionService, sessionManager, authProvider, botProfile),
      IBotCommandResolver<GroupUpdate>
{
    public async Task Handle(GroupUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);
        // Check the event type too: VkNet also uses MessageNew for edits and outgoing replies.
        if (update.Type?.Value == GroupUpdateType.MessageNew)
        {
            var message = update.Instance switch
            {
                MessageNew incoming => incoming.Message,
                Message legacy => legacy,
                _ => throw new ArgumentException("VK message_new must contain a message.", nameof(update))
            };
            // Chat service actions are not user replies and must not advance a session.
            if (message.Action != null) return;
            await QueryHandler(new VkMessageQueryContext(message), messageMatchers);
        }
        else if (update.Type?.Value == GroupUpdateType.MessageEvent)
        {
            if (update.Instance is not MessageEvent callback)
                throw new ArgumentException("VK message_event must contain a callback.", nameof(update));
            await QueryHandler(new VkMessageEventContext(callback), callbackMatchers);
        }
    }
}
