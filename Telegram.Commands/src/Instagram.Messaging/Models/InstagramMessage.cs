using System.Text.Json;
using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

public sealed class InstagramMessage
{
    [JsonPropertyName("mid")] public string? Mid { get; init; }
    [JsonPropertyName("text")] public string? Text { get; init; }
    [JsonPropertyName("is_echo")] public bool IsEcho { get; init; }
    [JsonPropertyName("is_deleted")] public bool IsDeleted { get; init; }
    [JsonPropertyName("attachments")] public InstagramAttachment[]? Attachments { get; init; }
    [JsonPropertyName("quick_reply")] public InstagramQuickReply? QuickReply { get; init; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
