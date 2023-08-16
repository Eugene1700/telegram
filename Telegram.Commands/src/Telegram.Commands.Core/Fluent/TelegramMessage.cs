using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent
{
    internal class TelegramMessage : ITelegramMessage
    {
        public TelegramMessage(IMessageText message, IReplyMarkup replyMarkup)
        {
            Text = message.Text;
            ParseMode = message.ParseMode;
            ReplyMarkup = replyMarkup;
        }

        public string Text { get; }
        public object ParseMode { get; }
        public IReplyMarkup ReplyMarkup { get; }
    }
}