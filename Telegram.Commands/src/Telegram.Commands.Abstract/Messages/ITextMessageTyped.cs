namespace Telegram.Commands.Abstract.Messages
{
    public interface ITextMessageTyped<out TParseMode> : ITextMessage
    {
        new TParseMode ParseMode { get; }
    }
}