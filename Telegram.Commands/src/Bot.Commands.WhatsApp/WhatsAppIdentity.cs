using System.Globalization;
namespace Bot.Commands.WhatsApp;

/// <summary>Checked mapping of numeric WhatsApp-scoped IDs to the existing session contracts.</summary>
public static class WhatsAppIdentity
{
    public static long ToSessionId(string? id)
    {
        if (string.IsNullOrEmpty(id) || id.Any(c => c < '0' || c > '9') ||
            !long.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value <= 0 ||
            value.ToString(CultureInfo.InvariantCulture) != id)
            throw new ArgumentException("WhatsApp ID must be a canonical positive Int64 decimal string. No hashing or truncation is performed.", nameof(id));
        return value;
    }
}
