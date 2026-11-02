using System;

namespace Bot.Commands.Core.Exceptions
{
    public class CommandChatAreaException : Exception, ICommandException
    {
        public CommandChatAreaException(string message, long chatId) : base(message)
        {
            ChatId = chatId;
        }
        public long ChatId { get; }
    }

    public interface ICommandException
    {
        public long ChatId { get; }
    }
}