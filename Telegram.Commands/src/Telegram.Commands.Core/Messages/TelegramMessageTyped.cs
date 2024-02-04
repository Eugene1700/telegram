using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Messages
{

    public class TelegramMessageTyped<TParseMode> : ITelegramMessageTyped<TParseMode>
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

        public byte[] Photo { get; set; }
    }
}