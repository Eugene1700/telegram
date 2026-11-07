using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

public sealed record InstagramQuickReply([property: JsonPropertyName("payload")] string? Payload);
