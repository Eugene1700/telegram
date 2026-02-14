namespace Telegram.Commands.Abstract.Messages
{
    public interface ITelegramMessageTyped<out TParseMode> : ITelegramMessage, ITextMessageTyped<TParseMode>
    {
        new TParseMode ParseMode { get; }
    }
}