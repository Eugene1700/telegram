using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramDomainException : Exception
    {
        public TelegramDomainException(string message) : base(message)
        {
        }
    }
}