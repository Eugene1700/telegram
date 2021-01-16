using System;

namespace Telegram.Commands.Core
{
    public class TelegramException : Exception
    {
        public TelegramException(string message) : base(message)
        {
        }
    }
}