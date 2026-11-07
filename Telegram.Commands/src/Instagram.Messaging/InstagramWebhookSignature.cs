using System.Security.Cryptography;
using System.Text;
namespace Instagram.Messaging;

public static class InstagramWebhookSignature
{
    public static bool IsValid(ReadOnlySpan<byte> body, string? signature, string? appSecret)
    {
        if (string.IsNullOrEmpty(appSecret) || signature?.Length != 71 ||
            !signature.StartsWith("sha256=", StringComparison.Ordinal)) return false;
        byte[] received;
        try { received = Convert.FromHexString(signature.AsSpan(7)); }
        catch (FormatException) { return false; }
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(appSecret), body);
        return CryptographicOperations.FixedTimeEquals(expected, received);
    }
}
