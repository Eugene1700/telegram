using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Realization.Extensions{

    public static class TelegramMessageExtensions
    {
        public static bool IsPhotoMessage(this IBotMessage telegramMessage)
        {
            return telegramMessage.Photo != null && telegramMessage.Photo.Any();
            ;
        }
        public static ParseMode? ToParseMode(this BotMessageParseMode parseMode)
        {
            switch (parseMode)
            {
                case BotMessageParseMode.Plain: return null;
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