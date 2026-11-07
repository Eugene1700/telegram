using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppMedia
{
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("mime_type")] public string? MimeType { get; init; }
    [JsonPropertyName("sha256")] public string? Sha256 { get; init; }
    [JsonPropertyName("caption")] public string? Caption { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
