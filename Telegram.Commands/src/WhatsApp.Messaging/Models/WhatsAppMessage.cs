using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppMessage
{
    [JsonPropertyName("from")] public string? From { get; init; }
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("timestamp")] public string? Timestamp { get; init; }
    [JsonPropertyName("text")] public WhatsAppText? Text { get; init; }
    [JsonPropertyName("image")] public WhatsAppMedia? Image { get; init; }
    [JsonPropertyName("interactive")] public WhatsAppInteractive? Interactive { get; init; }
    [JsonPropertyName("button")] public WhatsAppReply? Button { get; init; }
    [JsonPropertyName("context")] public WhatsAppMessageReference? Context { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
