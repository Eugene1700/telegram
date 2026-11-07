using global::Max.Bot.Types;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using Bot.Commands.Max.Models;

namespace Bot.Commands.Max;

/// <summary>Builds native Max inline keyboards using the shared row and metadata builder.</summary>
public class MaxInlineMarkupQueryBuilder
    : AdapterInlineMarkupQueryBuilder<MaxInlineMarkupQueryBuilder, CallbackQuery, InlineKeyboardButton, InlineKeyboard>
{
    protected override InlineKeyboardButton ToButton(CallbackDataWithCommand callback) => MaxMessageExtensions.ToButton(callback);
    protected override InlineKeyboard ToMarkup(InlineKeyboardButton[][] rows) => new(rows);
}
