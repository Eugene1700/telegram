using System.Text.Json;

namespace Bot.Commands.Vk;

/// <summary>Wraps command/Fluent data in the JSON object required by VK callback buttons.</summary>
public static class VkCallbackPayload
{
    public static string Encode(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return JsonSerializer.Serialize(new Dictionary<string, string> { ["bot_commands"] = data });
    }

    /// <summary>Only decodes this adapter's envelope. Other payloads remain available to application matchers.</summary>
    public static bool TryDecode(string? payload, out string data)
    {
        data = string.Empty;
        if (string.IsNullOrWhiteSpace(payload)) return false;
        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("bot_commands", out var value) ||
                value.ValueKind != JsonValueKind.String) return false;
            data = value.GetString()!;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
