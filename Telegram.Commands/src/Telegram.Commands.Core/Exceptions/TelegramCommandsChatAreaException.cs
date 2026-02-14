using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsChatAreaException : Exception, ITelegramCommandException
    {
        public TelegramCommandsChatAreaException(string message, long chatId) : base(message)
        {
            ChatId = chatId;
        }
        public long ChatId { get; }
    }

    public interface ITelegramCommandException
    {
        public long ChatId { get; }
    }
}