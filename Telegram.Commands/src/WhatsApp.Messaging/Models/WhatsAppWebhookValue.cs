using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppWebhookValue
{
    [JsonPropertyName("messaging_product")] public string? MessagingProduct { get; init; }
    [JsonPropertyName("metadata")] public WhatsAppMetadata? Metadata { get; init; }
    [JsonPropertyName("messages")] public WhatsAppMessage[]? Messages { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
