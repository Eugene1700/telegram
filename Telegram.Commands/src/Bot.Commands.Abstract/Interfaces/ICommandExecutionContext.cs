using System;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface ICommandExecutionContext<TQuery>
    {
        public TQuery Query { get; }
        public ICommandDescriptor CommandDescriptor { get;}
        public Type CommandType { get; }
        public long ChatId { get; }
    }
}