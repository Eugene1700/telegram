using System.Text.Json.Serialization;
namespace Instagram.Messaging;

public sealed record InstagramSendResult(
    [property: JsonPropertyName("recipient_id")] string RecipientId,
    [property: JsonPropertyName("message_id")] string MessageId);
