using System.Text.RegularExpressions;
namespace WhatsApp.Messaging;

public sealed class WhatsAppOptions
{
    public string PhoneNumberId { get; init; } = "";
    public string BusinessAccountId { get; init; } = "";
    public string AccessToken { get; init; } = "";
    public string ApiVersion { get; init; } = "";
    public void Validate()
    {
        ValidateId(PhoneNumberId);
        if (string.IsNullOrWhiteSpace(AccessToken)) throw new ArgumentException("WhatsApp AccessToken is required.");
        if (ApiVersion == null || !Regex.IsMatch(ApiVersion, @"^v[0-9]{1,3}\.[0-9]{1,2}$"))
            throw new ArgumentException("WhatsApp ApiVersion must have the form vNN.N.");
    }
    public static void ValidateId(string? id)
    {
        if (string.IsNullOrEmpty(id) || id[0] == '0' || id.Any(c => c < '0' || c > '9'))
            throw new ArgumentException("WhatsApp ID must be a positive decimal string.", nameof(id));
    }
}
