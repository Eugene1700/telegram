using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent
{

    internal class TextMessage: IMessageText
    {
        public TextMessage(string message, object parseMode)
        {
            Text = message;
            ParseMode = parseMode;
        }

        public string Text { get; }
        public object ParseMode { get; }
    }
    
    internal class TextMessageTyped<TParseMode>: IMessageTextTyped<TParseMode>
    {
        public TextMessageTyped(IMessageText messageText)
        {
            Text = messageText.Text;
            ParseMode = (TParseMode)messageText.ParseMode;
        }

        public string Text { get; }
        object IMessageText.ParseMode => ParseMode;

        public TParseMode ParseMode { get; }
    }
}