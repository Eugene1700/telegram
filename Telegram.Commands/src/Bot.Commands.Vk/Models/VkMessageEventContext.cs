using System.Globalization;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using VkNet.Model;

namespace Bot.Commands.Vk.Models;

/// <summary>A callback button event. The host owns acknowledgement through messages.sendMessageEventAnswer.</summary>
public class VkMessageEventContext(MessageEvent query) : IQueryContext<MessageEvent>
{
    public MessageEvent Query { get; } = query ?? throw new ArgumentNullException(nameof(query));
    public long GetChatId() => Query.PeerId ?? throw new InvalidOperationException("VK callback has no peer_id.");
    public long GetFromId() => Query.UserId ?? throw new InvalidOperationException("VK callback has no user_id.");
    public string GetData() => VkCallbackPayload.TryDecode(Query.Payload, out var data) ? data : Query.Payload;
    public bool IsGroupMessage() => GetChatId() >= 2_000_000_000;
    public ChatArea GetChatType() => IsGroupMessage() ? ChatArea.Group : ChatArea.Private;
    public bool IsEvent() => false;
    public EventType? GetEventType() => null;
    public string? GetMessageId() => Query.ConversationMessageId?.ToString(CultureInfo.InvariantCulture);
}
