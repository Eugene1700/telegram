using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Abstract.Messages
{
    public record CallbackDataWithCommand : CallbackData
    {
        public ICommandDescriptor CommandDescriptor { get; init; }
        public int Row { get; init; }
    }
}
