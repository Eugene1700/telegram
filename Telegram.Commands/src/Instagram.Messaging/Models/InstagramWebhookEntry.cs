using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

public sealed class InstagramWebhookEntry
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("time")] public long Time { get; init; }
    [JsonPropertyName("messaging")] public InstagramMessagingEvent[]? Messaging { get; init; }
}
