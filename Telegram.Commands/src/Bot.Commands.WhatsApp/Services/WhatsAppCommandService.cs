using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Services;
using Bot.Commands.WhatsApp.Models;
using WhatsApp.Messaging.Models;

namespace Bot.Commands.WhatsApp.Services;

/// <summary>Handles one inbound message; the host validates and splits webhook batches.</summary>
public sealed class WhatsAppCommandService(
    CommandExecutionService execution, SessionManager sessions, IAuthProvider auth, IBotProfile profile,
    IEnumerable<ICommandMatcher<WhatsAppMessage>>? messageMatchers = null,
    IEnumerable<ICommandMatcher<WhatsAppReply>>? replyMatchers = null)
    : CommandServiceBase(execution, sessions, auth, profile), IBotCommandResolver<WhatsAppMessage>
{
    public async Task Handle(WhatsAppMessage update)
    {
        ArgumentNullException.ThrowIfNull(update);
        var reply = update.Type switch
        {
            "button" => update.Button,
            "interactive" => update.Interactive?.Type switch
            {
                "button_reply" => update.Interactive.ButtonReply,
                "list_reply" => update.Interactive.ListReply,
                _ => null
            },
            _ => null
        };
        if (!string.IsNullOrEmpty(reply?.Id ?? reply?.Payload))
            await QueryHandler(new WhatsAppReplyQueryContext(update, reply), replyMatchers);
        else if ((update.Type == "text" && update.Text?.Body != null) ||
                 (update.Type == "image" && !string.IsNullOrEmpty(update.Image?.Id)))
            await QueryHandler(new WhatsAppMessageQueryContext(update), messageMatchers);
        // Statuses never reach this resolver; reactions, system and unsupported messages cannot advance sessions.
    }
}
