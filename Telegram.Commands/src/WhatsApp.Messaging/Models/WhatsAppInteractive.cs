using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppInteractive
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("button_reply")] public WhatsAppReply? ButtonReply { get; init; }
    [JsonPropertyName("list_reply")] public WhatsAppReply? ListReply { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
