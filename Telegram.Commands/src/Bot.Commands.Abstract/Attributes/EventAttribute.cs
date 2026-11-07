using System;

namespace Bot.Commands.Abstract.Attributes
{
    public class EventAttribute : CommandAttribute
    {
        public EventType Type { get; set; }
        public int Order { get; set; }
    }
}