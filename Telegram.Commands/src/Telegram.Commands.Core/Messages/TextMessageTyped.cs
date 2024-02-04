using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Messages
{

    internal class TextMessageTyped<TParseMode> : ITextMessageTyped<TParseMode>
    {
        public TextMessageTyped(ITextMessage textMessage)
        {
            Text = textMessage.Text;
            ParseMode = (TParseMode)textMessage.ParseMode;
        }

        public string Text { get; }
        object ITextMessage.ParseMode => ParseMode;

        public TParseMode ParseMode { get; }
    }
}