using Telegram.Commands.Core.Exceptions;

namespace Telegram.Commands.Core.Models
{
    internal class CommandDescriptorComposition
    {
        private CommandDescriptorComposition()
        {
        }

        public FullCommandDescriptor SessionCommand { get; private set; }
        public FullCommandDescriptor QueryCommand { get; private set; }

        public static CommandDescriptorComposition CreateSessionResult(FullCommandDescriptor sessionCommandDescriptor)
        {
            if (!sessionCommandDescriptor.IsSessionTelegramCommand)
                throw new TelegramExtractionCommandInternalException(
                    $"This command {sessionCommandDescriptor.Descriptor.Name} is not a session command");
            return new CommandDescriptorComposition
            {
                SessionCommand = sessionCommandDescriptor,
                QueryCommand = null
            };
        }

        public static CommandDescriptorComposition CreateBehaviorResult(FullCommandDescriptor sessionCommandDescriptor,
            FullCommandDescriptor queryCommandDescriptor)
        {
            if (!sessionCommandDescriptor.IsBehaviorTelegramCommand ||
                (!queryCommandDescriptor.IsQueryCommand && !queryCommandDescriptor.IsSessionTelegramCommand))
                throw new TelegramExtractionCommandInternalException("Incomapitable command types");
            return new CommandDescriptorComposition
            {
                SessionCommand = sessionCommandDescriptor,
                QueryCommand = queryCommandDescriptor
            };
        }
        
        public static CommandDescriptorComposition CreateBehaviorResult(FullCommandDescriptor sessionCommandDescriptor)
        {
            if (!sessionCommandDescriptor.IsBehaviorTelegramCommand)
                throw new TelegramExtractionCommandInternalException("Incompatable command types");
            return new CommandDescriptorComposition
            {
                SessionCommand = sessionCommandDescriptor,
                QueryCommand = null
            };
        }

        public static CommandDescriptorComposition CreateQueryResult(FullCommandDescriptor queryCommandDescriptor)
        {
            if (!queryCommandDescriptor.IsQueryCommand && !queryCommandDescriptor.IsBehaviorTelegramCommand)
                throw new TelegramExtractionCommandInternalException(
                    $"This command {queryCommandDescriptor.Descriptor.Name} is not a query command");
            return new CommandDescriptorComposition
            {
                SessionCommand = null,
                QueryCommand = queryCommandDescriptor
            };
        }

        public bool IsBehaviorCommand =>
            SessionCommand is { IsBehaviorTelegramCommand: true } ||
            QueryCommand is { IsBehaviorTelegramCommand: true };

        public bool IsSessionCommand =>
            SessionCommand is { IsSessionTelegramCommand: true } && QueryCommand == null;

        public bool IsQueryCommand =>
            QueryCommand is { IsQueryCommand: true } && SessionCommand == null;

        public bool Authorized => (SessionCommand?.Authorized ?? false) || (QueryCommand?.Authorized ?? false);
    }
}