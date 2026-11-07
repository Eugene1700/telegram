using System.Globalization;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using VkNet.Model;

namespace Bot.Commands.Vk.Models;

/// <summary>A VK message. Message IDs are conversation-local and must be paired with peer_id.</summary>
public class VkMessageQueryContext(Message query) : IQueryContext<Message>
{
    public Message Query { get; } = query ?? throw new ArgumentNullException(nameof(query));
    public long GetChatId() => Query.PeerId ?? throw new InvalidOperationException("VK message has no peer_id.");
    public long GetFromId() => Query.FromId ?? throw new InvalidOperationException("VK message has no from_id.");
    public string GetData() => VkCallbackPayload.TryDecode(Query.Payload, out var data) ? data : Query.Text;
    public bool IsGroupMessage() => GetChatId() >= 2_000_000_000;
    public ChatArea GetChatType() => IsGroupMessage() ? ChatArea.Group : ChatArea.Private;
    public bool IsEvent() => false;
    public EventType? GetEventType() => null;
    public string? GetMessageId() => Query.ConversationMessageId?.ToString(CultureInfo.InvariantCulture);
}
