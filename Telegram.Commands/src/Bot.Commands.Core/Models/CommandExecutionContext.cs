using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Models
{
    using System;

    internal class CommandExecutionContext<TQuery> : ICommandExecutionContext<TQuery>
    {
        public CommandExecutionContext(TQuery query, FullCommandDescriptor fullCommandDescriptor, long chatId)
        {
            Query = query;
            CommandDescriptor = fullCommandDescriptor.Descriptor;
            CommandType = fullCommandDescriptor.Type;
            ChatId = chatId;
        }

        public TQuery Query { get; }
        public ICommandDescriptor CommandDescriptor { get; }
        public Type CommandType { get; }
        public long ChatId { get; }
    }
}