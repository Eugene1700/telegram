using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Commands.Abstract.Messages
{
    public interface ITelegramMessage
    {
        string Text { get; }
        object ParseMode { get; }
        InlineKeyboardMarkup ReplyMarkup { get; }
        public byte[] Photo { get; set; }
    }
}