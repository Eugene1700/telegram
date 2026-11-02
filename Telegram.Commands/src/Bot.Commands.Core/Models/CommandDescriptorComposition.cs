using Bot.Commands.Core.Exceptions;

namespace Bot.Commands.Core.Models
{
    public class CommandDescriptorComposition
    {
        private CommandDescriptorComposition()
        {
        }

        public FullCommandDescriptor SessionCommand { get; private set; }
        public FullCommandDescriptor QueryCommand { get; private set; }

        public static CommandDescriptorComposition CreateSessionResult(FullCommandDescriptor sessionCommandDescriptor)
        {
            if (!sessionCommandDescriptor.IsSessionCommand)
                throw new CommandExtractionInternalException(
                    $"This command {sessionCommandDescriptor.Descriptor.Name} is not a session command");
            return new CommandDescriptorComposition
            {
                SessionCommand = sessionCommandDescriptor,
                QueryCommand = null
            };
        }

        public static CommandDescriptorComposition CreateBehaviorResult(FullCommandDescriptor behaviorCommandDescriptor,
            FullCommandDescriptor queryCommandDescriptor)
        {
            if (!behaviorCommandDescriptor.IsBehaviorCommand ||
                (!queryCommandDescriptor.IsQueryCommand && !queryCommandDescriptor.IsSessionCommand))
                throw new CommandExtractionInternalException("Incomapitable command types");
            return new CommandDescriptorComposition
            {
                SessionCommand = behaviorCommandDescriptor,
                QueryCommand = queryCommandDescriptor
            };
        }
        
        public static CommandDescriptorComposition CreateBehaviorResult(FullCommandDescriptor behaviorCommandDescriptor)
        {
            if (!behaviorCommandDescriptor.IsBehaviorCommand)
                throw new CommandExtractionInternalException("Incompatable command types");
            return new CommandDescriptorComposition
            {
                SessionCommand = behaviorCommandDescriptor,
                QueryCommand = null
            };
        }

        public static CommandDescriptorComposition CreateQueryResult(FullCommandDescriptor queryCommandDescriptor)
        {
            if (!queryCommandDescriptor.IsQueryCommand && !queryCommandDescriptor.IsBehaviorCommand)
                throw new CommandExtractionInternalException(
                    $"This command {queryCommandDescriptor.Descriptor.Name} is not a query command");
            return new CommandDescriptorComposition
            {
                SessionCommand = null,
                QueryCommand = queryCommandDescriptor
            };
        }

        public bool IsBehaviorCommand =>
            SessionCommand is { IsBehaviorCommand: true } ||
            QueryCommand is { IsBehaviorCommand: true };

        public bool IsSessionCommand =>
            SessionCommand is { IsSessionCommand: true } && QueryCommand == null;

        public bool IsQueryCommand =>
            QueryCommand is { IsQueryCommand: true } && SessionCommand == null;

        public bool Authorized => (SessionCommand?.Authorized ?? false) || (QueryCommand?.Authorized ?? false);
    }
}