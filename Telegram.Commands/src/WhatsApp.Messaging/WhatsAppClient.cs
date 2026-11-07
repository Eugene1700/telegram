using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WhatsApp.Messaging.Models;

namespace WhatsApp.Messaging;

/// <summary>Repository-owned client for the official WhatsApp Cloud API.</summary>
public sealed class WhatsAppClient : IWhatsAppClient
{
    private readonly HttpClient _http;
    private readonly WhatsAppOptions _options;
    public WhatsAppClient(HttpClient http, WhatsAppOptions options)
    {
        options.Validate();
        _http = http;
        _options = options;
    }
    public Task<string> SendTextAsync(string recipientId, string text, IReadOnlyList<WhatsAppReplyButton>? buttons = null,
        CancellationToken cancellationToken = default)
    {
        WhatsAppOptions.ValidateId(recipientId);
        ValidateText(text, buttons);
        object payload = buttons is { Count: > 0 }
            ? new { messaging_product = "whatsapp", recipient_type = "individual", to = recipientId, type = "interactive",
                interactive = new { type = "button", body = new { text }, action = new {
                    buttons = buttons.Select(b => new { type = "reply", reply = new { id = b.Id, title = b.Title } }).ToArray() } } }
            : new { messaging_product = "whatsapp", recipient_type = "individual", to = recipientId, type = "text",
                text = new { body = text, preview_url = false } };
        return Send(payload, cancellationToken);
    }
    public static void ValidateText(string text, IReadOnlyList<WhatsAppReplyButton>? buttons = null)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > (buttons is { Count: > 0 } ? 1024 : 4096))
            throw new ArgumentException("WhatsApp text is required and must fit the message type's length limit.", nameof(text));
        if (buttons?.Count > 3) throw new ArgumentException("WhatsApp supports at most three reply buttons.");
        foreach (var button in buttons ?? []) button.Validate();
        if (buttons != null && buttons.Select(b => b.Id).Distinct(StringComparer.Ordinal).Count() != buttons.Count)
            throw new ArgumentException("WhatsApp reply button IDs must be unique within a message.");
    }
    public Task<string> SendImageAsync(string recipientId, Uri imageUrl, CancellationToken cancellationToken = default)
    {
        WhatsAppOptions.ValidateId(recipientId);
        if (imageUrl == null || !imageUrl.IsAbsoluteUri || imageUrl.Scheme != "https")
            throw new ArgumentException("A public HTTPS image URL is required.", nameof(imageUrl));
        return Send(new { messaging_product = "whatsapp", recipient_type = "individual", to = recipientId,
            type = "image", image = new { link = imageUrl.AbsoluteUri } }, cancellationToken);
    }
    public async Task SubscribeToMessagingAsync(CancellationToken cancellationToken = default)
    {
        WhatsAppOptions.ValidateId(_options.BusinessAccountId);
        using var body = await Post(_options.BusinessAccountId + "/subscribed_apps", new { }, cancellationToken);
        if (!body.RootElement.TryGetProperty("success", out var success) || success.ValueKind != JsonValueKind.True)
            throw new JsonException("WhatsApp webhook subscription was not confirmed.");
    }
    private async Task<string> Send(object payload, CancellationToken cancellationToken)
    {
        using var body = await Post(_options.PhoneNumberId + "/messages", payload, cancellationToken);
        if (!body.RootElement.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array ||
            messages.GetArrayLength() == 0 || !messages[0].TryGetProperty("id", out var id) ||
            id.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(id.GetString()))
            throw new JsonException("WhatsApp Send API response has no message ID.");
        return id.GetString()!;
    }
    private async Task<JsonDocument> Post(string path, object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/{_options.ApiVersion}/{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        request.Content = JsonContent.Create(payload);
        using var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"WhatsApp API failed (HTTP {(int)response.StatusCode}).", null, response.StatusCode);
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
    }
}
