using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsDirectCallProhibitedException : Exception
    {
        public TelegramCommandsDirectCallProhibitedException(string message) : base(message)
        {
        }
    }
}