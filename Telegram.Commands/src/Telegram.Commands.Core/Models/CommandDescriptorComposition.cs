﻿using Telegram.Commands.Core.Exceptions;

namespace Telegram.Commands.Core.Models
{
    internal class CommandDescriptorComposition
    {
        private CommandDescriptorComposition()
        {
        }

        public FullCommandDescriptor SessionCommand { get; private init; }
        public FullCommandDescriptor QueryCommand { get; private init; }

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

        public static CommandDescriptorComposition CreateBehaviorResult(FullCommandDescriptor behaviorCommandDescriptor,
            FullCommandDescriptor queryCommandDescriptor)
        {
            if (!behaviorCommandDescriptor.IsBehaviorTelegramCommand ||
                (!queryCommandDescriptor.IsQueryCommand && !queryCommandDescriptor.IsSessionTelegramCommand))
                throw new TelegramExtractionCommandInternalException("Incomapitable command types");
            return new CommandDescriptorComposition
            {
                SessionCommand = behaviorCommandDescriptor,
                QueryCommand = queryCommandDescriptor
            };
        }
        
        public static CommandDescriptorComposition CreateBehaviorResult(FullCommandDescriptor behaviorCommandDescriptor)
        {
            if (!behaviorCommandDescriptor.IsBehaviorTelegramCommand)
                throw new TelegramExtractionCommandInternalException("Incompatable command types");
            return new CommandDescriptorComposition
            {
                SessionCommand = behaviorCommandDescriptor,
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