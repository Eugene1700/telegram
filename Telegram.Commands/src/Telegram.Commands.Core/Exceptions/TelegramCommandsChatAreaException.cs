using System;
using Telegram.Bot.Types;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsChatAreaException : Exception, ITelegramCommandException
    {
        public TelegramCommandsChatAreaException(string message, ChatId chatId) : base(message)
        {
            ChatId = chatId;
        }
        public ChatId ChatId { get; }
    }

    public interface ITelegramCommandException
    {
        public ChatId ChatId { get; }
    }
}