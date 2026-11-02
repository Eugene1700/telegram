using System;
using Bot.Commands.Abstract.Attributes;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface IEventDescriptor : ICommandDescriptor
    {
        public EventType Type { get; set; }
        public int Order { get; set; }
    }
}