using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract.Messages
{
    public record CallbackDataWithCommand : CallbackData
    {
        public ITelegramCommandDescriptor CommandDescriptor { get; init; }
    }
}