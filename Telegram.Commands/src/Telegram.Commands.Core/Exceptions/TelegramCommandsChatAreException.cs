using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsChatAreException : Exception
    {
        public TelegramCommandsChatAreException(string message) : base(message)
        {
        }
    }
}