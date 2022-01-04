using System;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramExtractionCommandInternalException : Exception
    {
        public TelegramExtractionCommandInternalException(string message) : base(message)
        {
        }
    }
}