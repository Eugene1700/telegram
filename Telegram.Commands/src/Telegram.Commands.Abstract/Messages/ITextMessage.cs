using System.Collections.ObjectModel;

namespace Telegram.Commands.Abstract.Messages
{
    public interface ITextMessage
    {
        string Text { get; }
        public byte[] Photo { get; }
        public BotMessageParseMode ParseMode { get;}
    }

    public interface IBotMessage : ITextMessage
    {
        public ReadOnlyCollection<CallbackDataWithCommand> Callbacks { get;}
    }
    
    public record BotMessage(string Text, byte[] Photo, BotMessageParseMode ParseMode, ReadOnlyCollection<CallbackDataWithCommand> Callbacks): IBotMessage;
}