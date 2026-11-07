using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using Instagram.Messaging.Models;

namespace Bot.Commands.Instagram;

/// <summary>Builds a button template's flat list. Instagram does not expose Telegram-style row layout.</summary>
public sealed class InstagramInlineMarkupQueryBuilder
    : AdapterInlineMarkupQueryBuilder<InstagramInlineMarkupQueryBuilder, InstagramPostback, InstagramButton, IReadOnlyList<InstagramButton>>
{
    protected override InstagramButton ToButton(CallbackDataWithCommand callback)
    {
        if (string.IsNullOrWhiteSpace(callback.Text)) throw new ArgumentException("Instagram button title is required.");
        if (callback.CallbackMode == CallbackMode.CallbackData && callback.CommandDescriptor == null &&
            string.IsNullOrEmpty(callback.CallbackText))
            throw new ArgumentException("Instagram postback payload is required.");
        return callback.CallbackMode switch
        {
            CallbackMode.CallbackData => new("postback", callback.Text,
                callback.CommandDescriptor == null ? callback.CallbackText : $"/{callback.CommandDescriptor.Name} {callback.CallbackText}"),
            CallbackMode.Url when Uri.TryCreate(callback.CallbackText, UriKind.Absolute, out var uri) &&
                                  uri.Scheme is "https" or "http" => new("web_url", callback.Text, Url: callback.CallbackText),
            CallbackMode.Url => throw new ArgumentException("Instagram buttons require an absolute HTTP(S) URL."),
            _ => throw new NotSupportedException($"Instagram does not support {callback.CallbackMode} buttons.")
        };
    }

    protected override IReadOnlyList<InstagramButton> ToMarkup(InstagramButton[][] rows)
    {
        var buttons = rows.SelectMany(row => row).ToArray();
        if (buttons.Length > 3) throw new NotSupportedException("An Instagram button template supports at most three buttons.");
        return buttons;
    }
}
