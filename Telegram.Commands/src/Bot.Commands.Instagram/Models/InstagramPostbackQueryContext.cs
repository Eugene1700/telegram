using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Instagram.Messaging.Models;
namespace Bot.Commands.Instagram.Models;

public sealed class InstagramPostbackQueryContext : IQueryContext<InstagramPostback>
{
    public InstagramMessagingEvent Event { get; }
    public InstagramPostback Query { get; }
    private readonly long _senderId;
    public InstagramPostbackQueryContext(InstagramMessagingEvent update)
    {
        Event = update ?? throw new ArgumentNullException(nameof(update));
        Query = update.Postback ?? throw new ArgumentException("Missing Instagram postback.", nameof(update));
        _senderId = InstagramIdentity.ToSessionId(update.Sender?.Id);
    }
    public long GetChatId() => _senderId;
    public long GetFromId() => _senderId;
    public string GetData() => Query.Payload!;
    public bool IsGroupMessage() => false;
    public ChatArea GetChatType() => ChatArea.Private;
    public bool IsEvent() => false;
    public EventType? GetEventType() => null;
    public string? GetMessageId() => Query.Mid;
}
