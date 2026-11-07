using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using WhatsApp.Messaging.Models;

namespace Bot.Commands.WhatsApp;

public sealed class WhatsAppInlineMarkupQueryBuilder
    : AdapterInlineMarkupQueryBuilder<WhatsAppInlineMarkupQueryBuilder, WhatsAppReply, WhatsAppReplyButton, IReadOnlyList<WhatsAppReplyButton>>
{
    protected override WhatsAppReplyButton ToButton(CallbackDataWithCommand callback)
    {
        if (callback.CallbackMode != CallbackMode.CallbackData)
            throw new NotSupportedException("This WhatsApp reply-button builder supports callback buttons only.");
        var button = new WhatsAppReplyButton(
            callback.CommandDescriptor == null ? callback.CallbackText :
                $"/{callback.CommandDescriptor.Name}" + (string.IsNullOrEmpty(callback.CallbackText) ? "" : " " + callback.CallbackText),
            callback.Text);
        button.Validate();
        return button;
    }
    protected override IReadOnlyList<WhatsAppReplyButton> ToMarkup(WhatsAppReplyButton[][] rows)
    {
        var buttons = rows.SelectMany(r => r).ToArray();
        if (buttons.Length > 3) throw new NotSupportedException("WhatsApp supports at most three reply buttons.");
        if (buttons.Select(b => b.Id).Distinct(StringComparer.Ordinal).Count() != buttons.Length)
            throw new ArgumentException("WhatsApp reply button IDs must be unique.");
        return buttons;
    }
}
