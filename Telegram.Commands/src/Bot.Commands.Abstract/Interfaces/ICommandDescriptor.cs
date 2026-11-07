using Bot.Commands.Abstract.Attributes;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface ICommandDescriptor
    {
        string Name { get; }
        public ChatArea Area { get; }
        public string[] Swarms { get; }
        bool Authorized { get; }
    }
}