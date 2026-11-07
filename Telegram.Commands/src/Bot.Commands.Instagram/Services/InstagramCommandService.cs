using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Services;
using Bot.Commands.Instagram.Models;
using Instagram.Messaging.Models;

namespace Bot.Commands.Instagram.Services;

/// <summary>Handles one native messaging event. The host validates the account and splits webhook batches into scopes.</summary>
public class InstagramCommandService(
    CommandExecutionService execution, SessionManager sessions, IAuthProvider auth, IBotProfile profile,
    IEnumerable<ICommandMatcher<InstagramMessage>>? messageMatchers = null,
    IEnumerable<ICommandMatcher<InstagramPostback>>? postbackMatchers = null)
    : CommandServiceBase(execution, sessions, auth, profile), IBotCommandResolver<InstagramMessagingEvent>
{
    public async Task Handle(InstagramMessagingEvent update)
    {
        ArgumentNullException.ThrowIfNull(update);
        if (update.Message is { IsEcho: true } or { IsDeleted: true }) return;
        if (update.Sender?.Id == update.Recipient?.Id) return;
        if (update.Postback is { Payload: not null })
            await QueryHandler(new InstagramPostbackQueryContext(update), postbackMatchers);
        else if (update.Message is { } message &&
                 (message.Text != null || message.Attachments?.Length > 0 || message.QuickReply?.Payload != null))
            await QueryHandler(new InstagramMessageQueryContext(update), messageMatchers);
        // Read receipts, reactions, edits, deletes, and unsupported events never advance sessions.
    }
}
