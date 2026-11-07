using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using WhatsApp.Messaging.Models;

namespace Bot.Commands.WhatsApp.Models;

public sealed class WhatsAppReplyQueryContext : IQueryContext<WhatsAppReply>
{
    public WhatsAppMessage Message { get; }
    public WhatsAppReply Query { get; }
    private readonly long _senderId;
    public WhatsAppReplyQueryContext(WhatsAppMessage message, WhatsAppReply reply)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Query = reply ?? throw new ArgumentNullException(nameof(reply));
        _senderId = WhatsAppIdentity.ToSessionId(message.From);
    }
    public long GetChatId() => _senderId;
    public long GetFromId() => _senderId;
    public string GetData() => (Query.Id ?? Query.Payload)!;
    public bool IsGroupMessage() => false;
    public ChatArea GetChatType() => ChatArea.Private;
    public bool IsEvent() => false;
    public EventType? GetEventType() => null;
    public string? GetMessageId() => Message.Context?.Id;
}
