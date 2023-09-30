using System;
using System.Linq;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent
{
    public interface ITelegramMessage
    {
        string Text { get; }
        object ParseMode { get; }
        IReplyMarkup ReplyMarkup { get; }
    }
    
    public interface IMessageTextTyped<out TParseMode>: IMessageText
    {
        TParseMode ParseMode { get; }
    }
    
    public interface ITelegramMessageTyped<out TParseMode>: ITelegramMessage
    {
        TParseMode ParseMode { get; }
    }

    public static class TelegramMessageExtensions
    {
        public static ITelegramMessageTyped<TParseMode> Cast<TParseMode>(this ITelegramMessage telegramMessage)
        {
            return new TelegramMessageTyped<TParseMode>(telegramMessage);
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

    public class TelegramMessageTyped<TParseMode>: ITelegramMessageTyped<TParseMode>
    {
        public TelegramMessageTyped()
        {
            
        }
        public TelegramMessageTyped(ITelegramMessage telegramMessage)
        {
            Text = telegramMessage.Text;
            ParseMode = (TParseMode)telegramMessage.ParseMode;
            ReplyMarkup = telegramMessage.ReplyMarkup;
        }
        
        public string Text { get; set; }
        object ITelegramMessage.ParseMode => ParseMode;

        public TParseMode ParseMode { get; set; }

        public IReplyMarkup ReplyMarkup { get; set; }
        
        public InlineKeyboardMarkup InlineKeyboardMarkup => (InlineKeyboardMarkup) ReplyMarkup;

        public byte[] Photo { get; set; }
        public bool IsPhotoMessage => Photo != null && Photo.Any();
    }
}