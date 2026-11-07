using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

public sealed class InstagramWebhookUpdate
{
    [JsonPropertyName("object")] public string? Object { get; init; }
    [JsonPropertyName("entry")] public InstagramWebhookEntry[]? Entries { get; init; }
}
