using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramExtractionCommandException : Exception
    {
        public TelegramExtractionCommandException(string message) : base(message)
        {
        }
    }
}