using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsPermissionException : Exception
    {
        public TelegramCommandsPermissionException(string message) : base(message)
        {
        }
    }
}