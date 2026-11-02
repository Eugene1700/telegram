using global::Max.Bot.Types;
using global::Max.Bot.Types.Enums;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Max.Models;

public static class MaxMessageExtensions
{
    public static InlineKeyboard ToInlineKeyboard(this IBotMessage message)
    {
        return BuildMarkup(message.Callbacks);
    }

    public static InlineKeyboard GetResult(this global::Bot.Commands.Core.Services.InlineMarkupQueryBuilder builder)
        => BuildMarkup(builder.GetCallbacks());

    private static InlineKeyboard BuildMarkup(IEnumerable<CallbackDataWithCommand>? callbacks)
    {
        var builder = new MaxInlineMarkupQueryBuilder();
        foreach (var row in (callbacks ?? Enumerable.Empty<CallbackDataWithCommand>()).GroupBy(c => c.Row))
            builder.InlineKeyboardButtonsRow(row.ToArray());
        return builder.GetResult();
    }

    public static TextFormat? ToTextFormat(this BotMessageParseMode mode) => mode switch
    {
        BotMessageParseMode.Plain => null,
        BotMessageParseMode.MarkDown => TextFormat.Markdown,
        BotMessageParseMode.Html => TextFormat.Html,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    internal static InlineKeyboardButton ToButton(CallbackDataWithCommand callback) => callback.CallbackMode switch
    {
        CallbackMode.CallbackData => new InlineKeyboardButton
        {
            Text = callback.Text, Type = ButtonType.Callback,
            Payload = callback.CommandDescriptor == null ? callback.CallbackText
                : $"/{callback.CommandDescriptor.Name} {callback.CallbackText}"
        },
        CallbackMode.Url => new InlineKeyboardButton
        {
            Text = callback.Text, Type = ButtonType.Link, Url = callback.CallbackText
        },
        _ => throw new NotSupportedException($"Max does not support {callback.CallbackMode} buttons")
    };
}
