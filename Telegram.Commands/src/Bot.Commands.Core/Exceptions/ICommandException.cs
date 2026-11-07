using System;

namespace Bot.Commands.Core.Exceptions
{

    public interface ICommandException
    {
        public long ChatId { get; }
    }
}