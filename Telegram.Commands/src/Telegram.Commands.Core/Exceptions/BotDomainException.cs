using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class BotDomainException : Exception
    {
        public BotDomainException(string message) : base(message)
        {
        }
    }
}