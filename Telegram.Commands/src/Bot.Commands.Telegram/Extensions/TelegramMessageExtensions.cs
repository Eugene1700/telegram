using global::Telegram.Bot.Types.Enums;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Telegram.Extensions{

    public static class TelegramMessageExtensions
    {
        public static global::Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup ToReplyMarkup(this IBotMessage message)
            => BuildMarkup(message.Callbacks);

        public static global::Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup GetResult(
            this global::Bot.Commands.Core.Services.InlineMarkupQueryBuilder builder) => BuildMarkup(builder.GetCallbacks());

        private static global::Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup BuildMarkup(
            IEnumerable<CallbackDataWithCommand>? callbacks)
        {
            var builder = new TelegramInlineMarkupQueryBuilder();
            foreach (var row in (callbacks ?? Enumerable.Empty<CallbackDataWithCommand>()).GroupBy(c => c.Row))
                builder.InlineKeyboardButtonsRow(row.ToArray());
            return builder.GetResult();
        }

        public static bool IsPhotoMessage(this IBotMessage telegramMessage)
        {
            return telegramMessage.Photo != null && telegramMessage.Photo.Any();
            ;
        }
        public static ParseMode ToParseMode(this BotMessageParseMode parseMode)
        {
            switch (parseMode)
            {
                case BotMessageParseMode.Plain: return ParseMode.None;
                case BotMessageParseMode.MarkDown:
                    return ParseMode.MarkdownV2;
                case BotMessageParseMode.Html:
                    return ParseMode.Html;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parseMode), parseMode, null);
            }
        }
    }
}
