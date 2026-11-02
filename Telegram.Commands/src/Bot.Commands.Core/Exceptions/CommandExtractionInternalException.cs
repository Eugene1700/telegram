using System;

namespace Bot.Commands.Core.Exceptions
{
    public class CommandExtractionInternalException : Exception
    {
        public CommandExtractionInternalException(string message) : base(message)
        {
        }
    }
}