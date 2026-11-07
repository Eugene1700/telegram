using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppText
{
    [JsonPropertyName("body")] public string? Body { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
