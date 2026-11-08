using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using global::Max.Bot.Types;

namespace Bot.Commands.Max.Models;

/// <summary>A private-dialog start event. Payload is exposed unchanged, not interpreted as a slash command.</summary>
public sealed class BotStartedUpdateContext(BotStartedUpdate query) : IQueryContext<BotStartedUpdate>
{
    public BotStartedUpdate Query { get; } = query ?? throw new ArgumentNullException(nameof(query));
    public long GetChatId() => Query.ChatId;
    public long GetFromId() => Query.User.Id;
    public string GetData() => Query.Payload ?? string.Empty;
    public bool IsGroupMessage() => false;
    public bool IsEvent() => true;
    public EventType? GetEventType() => EventType.BotStarted;
    public ChatArea GetChatType() => ChatArea.Private;
    public string? GetMessageId() => null;
}
