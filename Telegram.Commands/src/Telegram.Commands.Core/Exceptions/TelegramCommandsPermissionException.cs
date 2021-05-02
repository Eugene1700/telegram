using System;
using Telegram.Bot.Types;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsPermissionException : Exception, ITelegramCommandException
    {
        public TelegramCommandsPermissionException(string message, ChatId chatId) : base(message)
        {
            ChatId = chatId;
        }

        public ChatId ChatId { get; }
    }
}