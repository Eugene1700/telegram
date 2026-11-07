using System.Text.RegularExpressions;
namespace Instagram.Messaging;

/// <summary>Instagram Login configuration. Supply an explicit supported Graph API version.</summary>
public sealed class InstagramOptions
{
    public string AccountId { get; init; } = "";
    public string AccessToken { get; init; } = "";
    public string ApiVersion { get; init; } = "";

    public void Validate()
    {
        ValidateId(AccountId);
        if (string.IsNullOrWhiteSpace(AccessToken)) throw new ArgumentException("Instagram AccessToken is required.");
        if (!Regex.IsMatch(ApiVersion, @"^v[0-9]{1,3}\.[0-9]{1,2}$"))
            throw new ArgumentException("Instagram ApiVersion must have the form vNN.N.");
    }

    internal static void ValidateId(string id)
    {
        if (string.IsNullOrEmpty(id) || id[0] == '0' || id.Any(c => c < '0' || c > '9'))
            throw new ArgumentException("Instagram ID must be a positive decimal string.", nameof(id));
    }
}
