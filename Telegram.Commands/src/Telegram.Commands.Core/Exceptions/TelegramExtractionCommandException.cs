using System;
using Telegram.Bot.Types;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramExtractionCommandException : Exception, ITelegramCommandException
    {
        public TelegramExtractionCommandException(string message, ChatId chatId) : base(message)
        {
            ChatId = chatId;
        }

        public ChatId ChatId { get; }
    }
}