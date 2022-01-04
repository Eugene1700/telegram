using System;
using Telegram.Commands.Abstract.Attributes;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramEventDescriptor : ITelegramCommandDescriptor
    {
        public EventType Type { get; set; }
        public int Order { get; set; }
    }
}