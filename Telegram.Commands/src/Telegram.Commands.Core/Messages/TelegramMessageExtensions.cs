using System;
using System.Linq;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent;

namespace Telegram.Commands.Core.Messages{

    public static class TelegramMessageExtensions
    {
        public static ITelegramMessageTyped<TParseMode> Cast<TParseMode>(this ITelegramMessage telegramMessage)
        {
            return new TelegramMessageTyped<TParseMode>(telegramMessage);
        }

        public static bool IsPhotoMessage(this ITelegramMessage telegramMessage)
        {
            return telegramMessage.Photo != null && telegramMessage.Photo.Any();
            ;
        }

        public static InlineKeyboardMarkup GetInlineKeyboardMarkup(this ITelegramMessage telegramMessage)
        {
            return (InlineKeyboardMarkup)telegramMessage.ReplyMarkup;
        }

        public static ParseMode? ToParseMode(this TelegramParseMode? parseMode)
        {
            return parseMode == null ? null : ToParseMode(parseMode.Value);
        }
        
        public static ParseMode? ToParseMode(this TelegramParseMode parseMode)
        {
            switch (parseMode)
            {
                case TelegramParseMode.Plain: return null;
                case TelegramParseMode.MarkDownV2:
                    return ParseMode.MarkdownV2;
                case TelegramParseMode.MarkDown:
                    return ParseMode.Markdown;
                case TelegramParseMode.Html:
                    return ParseMode.Html;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parseMode), parseMode, null);
            }
        }
    }
}