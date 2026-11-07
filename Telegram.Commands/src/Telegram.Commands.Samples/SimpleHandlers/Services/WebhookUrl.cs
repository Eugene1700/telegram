using System;

namespace SimpleHandlers.Services;

public static class WebhookUrl
{
    public static string Build(string baseUrl, string route)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidOperationException("Webhook BaseUrl must be an absolute HTTPS URL without credentials, query or fragment.");
        return new Uri(new Uri(uri.AbsoluteUri.TrimEnd('/') + "/"), route).AbsoluteUri;
    }
}
