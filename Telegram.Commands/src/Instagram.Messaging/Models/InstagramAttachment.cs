using System.Text.Json;
using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

/// <summary>Native attachment payload is retained; media URLs are not downloaded by the adapter.</summary>
public sealed class InstagramAttachment
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("payload")] public JsonElement Payload { get; init; }
}
