using System;

namespace Telegram.Commands.Abstract.Interfaces
{
    public class ITelegramEventDescriptor : ITelegramCommandDescriptor
    {
        public string Name { get; }
        public ChatArea Area { get; set; }
        public string[] Swarms { get; set; }

        public EventType Type { get; set; }
        public int Order { get; set; }
    }
}