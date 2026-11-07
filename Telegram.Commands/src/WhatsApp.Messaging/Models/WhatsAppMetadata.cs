using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatsApp.Messaging.Models;

public sealed class WhatsAppMetadata
{
    [JsonPropertyName("phone_number_id")] public string? PhoneNumberId { get; init; }
    [JsonPropertyName("display_phone_number")] public string? DisplayPhoneNumber { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
