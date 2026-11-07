using System.Globalization;
namespace Bot.Commands.Instagram;

/// <summary>Checked mapping of numeric Instagram-scoped IDs to the existing session contracts.</summary>
public static class InstagramIdentity
{
    public static long ToSessionId(string? id)
    {
        if (string.IsNullOrEmpty(id) || id.Any(c => c < '0' || c > '9') ||
            !long.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value <= 0 ||
            value.ToString(CultureInfo.InvariantCulture) != id)
            throw new ArgumentException("Instagram ID must be a canonical positive Int64 decimal string. No hashing or truncation is performed.", nameof(id));
        return value;
    }
}
