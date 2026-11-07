using System.Text.Json.Serialization;
namespace Instagram.Messaging.Models;

public sealed record InstagramParticipant([property: JsonPropertyName("id")] string Id);
