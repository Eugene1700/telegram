using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Instagram.Messaging.Models;
namespace Bot.Commands.Instagram.Models;

public sealed class InstagramMessageQueryContext : IQueryContext<InstagramMessage>
{
    public InstagramMessagingEvent Event { get; }
    public InstagramMessage Query { get; }
    private readonly long _senderId;
    public InstagramMessageQueryContext(InstagramMessagingEvent update)
    {
        Event = update ?? throw new ArgumentNullException(nameof(update));
        Query = update.Message ?? throw new ArgumentException("Missing Instagram message.", nameof(update));
        _senderId = InstagramIdentity.ToSessionId(update.Sender?.Id);
    }
    public long GetChatId() => _senderId;
    public long GetFromId() => _senderId;
    public string GetData() => Query.QuickReply?.Payload ?? Query.Text!;
    public bool IsGroupMessage() => false;
    public ChatArea GetChatType() => ChatArea.Private;
    public bool IsEvent() => false;
    public EventType? GetEventType() => null;
    public string? GetMessageId() => Query.Mid;
}
