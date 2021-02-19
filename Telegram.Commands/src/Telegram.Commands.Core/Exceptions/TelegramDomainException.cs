using System;

namespace Telegram.Commands.Core
{
    public class TelegramDomainException : Exception
    {
        public TelegramDomainException(string message) : base(message)
        {
        }
    }
}