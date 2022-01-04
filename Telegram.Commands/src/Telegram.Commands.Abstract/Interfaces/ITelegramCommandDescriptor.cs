using Telegram.Commands.Abstract.Attributes;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommandDescriptor
    {
        string Name { get; }
        public ChatArea Area { get; }
        public string[] Swarms { get; set; }
        bool Authorized { get; set; }
    }
}