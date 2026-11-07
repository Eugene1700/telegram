using System.Text.Json;
using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

public sealed class InstagramPostback
{
    [JsonPropertyName("mid")] public string? Mid { get; init; }
    [JsonPropertyName("title")] public string? Title { get; init; }
    [JsonPropertyName("payload")] public string? Payload { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
