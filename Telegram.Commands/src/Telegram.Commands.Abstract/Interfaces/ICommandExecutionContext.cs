using System;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ICommandExecutionContext<TQuery>
    {
        public TQuery Query { get; }
        public ITelegramCommandDescriptor CommandDescriptor { get;}
        public Type CommandType { get; }
        public long ChatId { get; }
    }
}