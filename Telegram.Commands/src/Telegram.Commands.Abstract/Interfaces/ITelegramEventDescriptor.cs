using System;

namespace Telegram.Commands.Abstract.Interfaces
{
    public class ITelegramEventDescriptor : ITelegramCommandDescriptor
    {
        public string Name { get; }
        public Permissions Permission { get; }
        public ChatArea Area { get; set; }
        public Type[] Reaction { get; set; }
        public Type[] By { get; set; }

        public EventType Type { get; set; }
        
        public int Order { get; set; }
    }
}