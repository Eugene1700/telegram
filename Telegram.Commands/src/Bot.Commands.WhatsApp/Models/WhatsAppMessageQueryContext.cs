using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using WhatsApp.Messaging.Models;

namespace Bot.Commands.WhatsApp.Models;

public sealed class WhatsAppMessageQueryContext : IQueryContext<WhatsAppMessage>
{
    public WhatsAppMessage Message { get; }
    public WhatsAppMessage Query { get; }
    private readonly long _senderId;
    public WhatsAppMessageQueryContext(WhatsAppMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Query = message;
        _senderId = WhatsAppIdentity.ToSessionId(message.From);
    }
    public long GetChatId() => _senderId;
    public long GetFromId() => _senderId;
    public string GetData() => (Query.Text?.Body ?? Query.Image?.Caption)!;
    public bool IsGroupMessage() => false;
    public ChatArea GetChatType() => ChatArea.Private;
    public bool IsEvent() => false;
    public EventType? GetEventType() => null;
    public string? GetMessageId() => Message.Id;
}
