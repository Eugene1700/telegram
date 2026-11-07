using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppMessageReference
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("from")] public string? From { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
