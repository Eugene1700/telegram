using System;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Models
{
    internal class CommandExecutionContext<TQuery> : ICommandExecutionContext<TQuery>
    {
        public CommandExecutionContext(TQuery query, FullCommandDescriptor fullCommandDescriptor)
        {
            Query = query;
            CommandDescriptor = fullCommandDescriptor.Descriptor;
            CommandType = fullCommandDescriptor.Type;
            ChatId = query.GetChatId();
        }
        public TQuery Query { get; }
        public ITelegramCommandDescriptor CommandDescriptor { get; }
        public Type CommandType { get; }
        public long ChatId { get; }
    }
}