using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Commands.Core.Fluent;

internal class TelegramMessage : ITelegramMessage
{
    public TelegramMessage(string message, IReplyMarkup replyMarkup)
    {
        Message = message;
        ReplyMarkup = replyMarkup;
    }

    public string Message { get; }

    public IReplyMarkup ReplyMarkup { get; }
}