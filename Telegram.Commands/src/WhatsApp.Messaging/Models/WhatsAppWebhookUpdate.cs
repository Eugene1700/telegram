using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppWebhookUpdate
{
    [JsonPropertyName("object")] public string? Object { get; init; }
    [JsonPropertyName("entry")] public WhatsAppWebhookEntry[]? Entries { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
