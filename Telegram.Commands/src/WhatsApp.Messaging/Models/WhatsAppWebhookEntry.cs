using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppWebhookEntry
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("changes")] public WhatsAppWebhookChange[]? Changes { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
