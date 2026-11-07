using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppWebhookChange
{
    [JsonPropertyName("field")] public string? Field { get; init; }
    [JsonPropertyName("value")] public WhatsAppWebhookValue? Value { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
