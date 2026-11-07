using Bot.Commands.Abstract.Messages;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace Bot.Commands.Vk.Models;

public static class VkMessageExtensions
{
    public static MessageKeyboard? ToInlineKeyboard(this IBotMessage message) => BuildMarkup(message.Callbacks);

    public static MessageKeyboard? GetResult(this global::Bot.Commands.Core.Services.InlineMarkupQueryBuilder builder)
        => BuildMarkup(builder.GetCallbacks());

    private static MessageKeyboard? BuildMarkup(this IEnumerable<CallbackDataWithCommand>? callbacks)
    {
        var buttons = callbacks?.ToArray();
        if (buttons == null || buttons.Length == 0) return null;
        var builder = new VkInlineMarkupQueryBuilder();
        foreach (var row in buttons.GroupBy(c => c.Row))
            builder.InlineKeyboardButtonsRow(row.ToArray());
        return builder.GetResult();
    }

    /// <summary>VK messages use plain text; reject formatting belonging to another messenger.</summary>
    public static string ToVkText(this IBotMessage message)
    {
        if (message.Format != null)
            throw new NotSupportedException($"Text format '{message.Format.GetType().Name}' is not supported by VK.");
        return message.Text;
    }

    internal static MessageKeyboardButton ToButton(this CallbackDataWithCommand callback) => callback.CallbackMode switch
    {
        CallbackMode.CallbackData => new MessageKeyboardButton
        {
            Action = new MessageKeyboardButtonAction
            {
                Type = KeyboardButtonActionType.Callback,
                Label = callback.Text,
                Payload = VkCallbackPayload.Encode(callback.CommandDescriptor == null ? callback.CallbackText
                    : $"/{callback.CommandDescriptor.Name} {callback.CallbackText}")
            }
        },
        CallbackMode.Url => new MessageKeyboardButton
        {
            Action = new MessageKeyboardButtonAction
            {
                Type = KeyboardButtonActionType.OpenLink,
                Label = callback.Text,
                Link = new Uri(callback.CallbackText, UriKind.Absolute)
            }
        },
        _ => throw new NotSupportedException($"VK does not support {callback.CallbackMode} buttons.")
    };
}
