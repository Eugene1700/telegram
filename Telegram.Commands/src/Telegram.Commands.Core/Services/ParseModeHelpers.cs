using System;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Messages;

namespace Telegram.Commands.Core.Services
{
    public static class ParseModeHelpers
    {
        public static (string, TelegramParseMode) CreatePlainMessage(this string text)
        {
            return (text, TelegramParseMode.Plain);
        }
        
        public static (string, TelegramParseMode) CreateCreateMarkdownMessage(this string text)
        {
            return (text, TelegramParseMode.MarkDownV2);
        }
        
        public static bool IsMarkdown(this TelegramParseMode telegramParseMode)
        {
            switch (telegramParseMode)
            {
                case TelegramParseMode.Plain:
                    return false;
                case TelegramParseMode.MarkDownV2:
                case TelegramParseMode.MarkDown:
                    return true;
                case TelegramParseMode.Html:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(telegramParseMode), telegramParseMode, null);
            }
        }
    }
}