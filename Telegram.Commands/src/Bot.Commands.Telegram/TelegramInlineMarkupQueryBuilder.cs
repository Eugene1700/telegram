using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.ReplyMarkups;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Telegram;

public class TelegramInlineMarkupQueryBuilder
    : AdapterInlineMarkupQueryBuilder<TelegramInlineMarkupQueryBuilder, CallbackQuery, InlineKeyboardButton, InlineKeyboardMarkup>
{
    protected override InlineKeyboardMarkup ToMarkup(InlineKeyboardButton[][] rows) => new(rows);

    // Preserve the validation rules of the existing array/typed overloads.
    protected override void ValidateButtons(CallbackData[] callbacks, ICommandDescriptor descriptor)
    {
        if (callbacks.Any(callback => descriptor == null
                ? callback.CallbackMode == CallbackMode.CallbackData && callback.CallbackText.Length > 64
                : callback.CallbackText.Length > 64 - descriptor.Name.Length - 2))
            throw new CommandsInternalException("Сallback message is too long");
    }

    protected override InlineKeyboardButton ToButton(CallbackDataWithCommand callback) => callback.CallbackMode switch
    {
        CallbackMode.CallbackData => InlineKeyboardButton.WithCallbackData(callback.Text,
            callback.CommandDescriptor == null ? callback.CallbackText
                : $"/{callback.CommandDescriptor.Name} {callback.CallbackText}"),
        CallbackMode.InlineCurrent => InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(callback.Text, callback.CallbackText),
        CallbackMode.Inline => InlineKeyboardButton.WithSwitchInlineQuery(callback.Text, callback.CallbackText),
        CallbackMode.Url => InlineKeyboardButton.WithUrl(callback.Text, callback.CallbackText),
        CallbackMode.Payment => InlineKeyboardButton.WithPay(callback.Text),
        _ => throw new ArgumentOutOfRangeException(nameof(callback.CallbackMode))
    };
}
