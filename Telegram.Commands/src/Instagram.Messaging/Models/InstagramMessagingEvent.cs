using System.Text.Json;
using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

public sealed class InstagramMessagingEvent
{
    [JsonPropertyName("sender")] public InstagramParticipant? Sender { get; init; }
    [JsonPropertyName("recipient")] public InstagramParticipant? Recipient { get; init; }
    [JsonPropertyName("timestamp")] public long Timestamp { get; init; }
    [JsonPropertyName("message")] public InstagramMessage? Message { get; init; }
    [JsonPropertyName("postback")] public InstagramPostback? Postback { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
