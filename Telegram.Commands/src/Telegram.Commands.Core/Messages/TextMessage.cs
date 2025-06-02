using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Messages
{
    internal class TextMessage: ITextMessage
    {
        public TextMessage(string message, object parseMode, byte[] photo)
        {
            Text = message;
            ParseMode = parseMode;
            Photo = photo;
        }

        public string Text { get; }
        public object ParseMode { get; }
        public byte[] Photo { get; set; }
    }
}