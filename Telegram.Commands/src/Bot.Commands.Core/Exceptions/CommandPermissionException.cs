using System;

namespace Bot.Commands.Core.Exceptions
{
    public class CommandPermissionException : Exception, ICommandException
    {
        public CommandPermissionException(string message, long chatId) : base(message)
        {
            ChatId = chatId;
        }

        public long ChatId { get; }
    }
}