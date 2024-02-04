using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Messages
{
    internal class TextMessage: ITextMessage
    {
        public TextMessage(string message, object parseMode)
        {
            Text = message;
            ParseMode = parseMode;
        }

        public string Text { get; }
        public object ParseMode { get; }
    }
}