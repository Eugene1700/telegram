using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsPermissionException : Exception, ITelegramCommandException
    {
        public TelegramCommandsPermissionException(string message, long chatId) : base(message)
        {
            ChatId = chatId;
        }

        public long ChatId { get; }
    }
}