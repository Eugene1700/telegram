namespace Telegram.Commands.Abstract.Messages
{
    public interface ITelegramMessageTyped<out TParseMode> : ITelegramMessage
    {
        new TParseMode ParseMode { get; }
    }
}