using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Messages
{
    internal class TelegramMessage : ITelegramMessage
    {
        public TelegramMessage(ITextMessage textMessage, IReplyMarkup replyMarkup)
        {
            Text = textMessage.Text;
            ParseMode = textMessage.ParseMode;
            ReplyMarkup = replyMarkup;
        }

        public string Text { get; }
        public object ParseMode { get; }
        public IReplyMarkup ReplyMarkup { get; }
        public byte[] Photo { get; set; }
    }
}