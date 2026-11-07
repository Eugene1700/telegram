using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Instagram.Messaging.Models;

namespace Instagram.Messaging;

/// <summary>Small client for the official Instagram Login Send API; no private API or password login.</summary>
public sealed class InstagramClient : IInstagramClient
{
    private readonly HttpClient _http;
    private readonly InstagramOptions _options;
    public InstagramClient(HttpClient http, InstagramOptions options)
    {
        options.Validate();
        _http = http;
        _options = options;
    }

    public Task<InstagramSendResult> SendTextAsync(string recipientId, string text,
        IReadOnlyList<InstagramButton>? buttons = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Instagram message text is required.", nameof(text));
        if (buttons?.Count > 3) throw new ArgumentException("Instagram button templates allow at most three buttons.", nameof(buttons));
        foreach (var button in buttons ?? [])
        {
            if (string.IsNullOrWhiteSpace(button.Title) ||
                (button.Type == "postback" && string.IsNullOrEmpty(button.Payload)) ||
                (button.Type == "web_url" && (!Uri.TryCreate(button.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))) ||
                button.Type is not ("postback" or "web_url"))
                throw new ArgumentException("Invalid Instagram button.", nameof(buttons));
        }
        object message = buttons is { Count: > 0 }
            ? new { attachment = new { type = "template", payload = new { template_type = "button", text, buttons } } }
            : new { text };
        return Send(recipientId, message, cancellationToken);
    }

    public Task<InstagramSendResult> SendImageAsync(string recipientId, Uri imageUrl, CancellationToken cancellationToken = default)
    {
        if (!imageUrl.IsAbsoluteUri || imageUrl.Scheme != "https")
            throw new ArgumentException("A publicly accessible HTTPS image URL is required.", nameof(imageUrl));
        return Send(recipientId, new { attachment = new { type = "image", payload = new { url = imageUrl.AbsoluteUri } } }, cancellationToken);
    }

    /// <summary>Subscribes the token's account; app-level callback configuration remains in the Meta dashboard.</summary>
    public async Task SubscribeToMessagingAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.instagram.com/{_options.ApiVersion}/me/subscribed_apps");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["subscribed_fields"] = "messages,messaging_postbacks"
        });
        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Instagram webhook subscription failed (HTTP {(int)response.StatusCode}).", null, response.StatusCode);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        if (!body.RootElement.TryGetProperty("success", out var success) || success.ValueKind != JsonValueKind.True)
            throw new JsonException("Instagram webhook subscription was not confirmed.");
    }

    private async Task<InstagramSendResult> Send(string recipientId, object message, CancellationToken cancellationToken)
    {
        InstagramOptions.ValidateId(recipientId);
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.instagram.com/{_options.ApiVersion}/{_options.AccountId}/messages");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        request.Content = JsonContent.Create(new { recipient = new { id = recipientId }, message });
        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Instagram Send API failed (HTTP {(int)response.StatusCode}).", null, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<InstagramSendResult>(cancellationToken: cancellationToken);
        if (string.IsNullOrEmpty(result?.MessageId) || string.IsNullOrEmpty(result.RecipientId))
            throw new JsonException("Instagram Send API response has no message_id or recipient_id.");
        return result;
    }
}
