using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramExtractionCommandException : Exception, ITelegramCommandException
    {
        public TelegramExtractionCommandException(string message, long chatId) : base(message)
        {
            ChatId = chatId;
        }

        public long ChatId { get; }
    }
}