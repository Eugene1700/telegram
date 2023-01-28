using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Commands.Core.Fluent;

public interface ITelegramMessage
{
    string Message { get; }
    IReplyMarkup ReplyMarkup { get; }
}