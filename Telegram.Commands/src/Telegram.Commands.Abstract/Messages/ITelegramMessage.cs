using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Commands.Abstract.Messages
{
    public interface ITelegramMessage: ITextMessage
    {
        IReplyMarkup ReplyMarkup { get; }
    }
}