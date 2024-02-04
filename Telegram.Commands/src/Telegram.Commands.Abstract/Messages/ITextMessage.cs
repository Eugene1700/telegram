namespace Telegram.Commands.Abstract.Messages
{

    public interface ITextMessage
    {
        string Text { get; }
        object ParseMode { get; }
    }
}