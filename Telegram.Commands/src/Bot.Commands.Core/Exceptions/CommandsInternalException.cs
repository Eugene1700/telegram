using System;

namespace Bot.Commands.Core.Exceptions
{
    public class CommandsInternalException : Exception
    {
        public CommandsInternalException(string message) : base(message)
        {
        }
    }
}