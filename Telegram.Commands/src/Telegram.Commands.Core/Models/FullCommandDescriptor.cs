using System;
using System.Linq;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Models
{
    internal class FullCommandDescriptor
    {
        public FullCommandDescriptor(Type type)
        {
            Type = type;
            Descriptor = TelegramCommandExtensions.GetCommandInfo(type);
        }

        public ITelegramCommandDescriptor Descriptor { get; }
        public Type Type { get; }

        public bool IsQueryCommand => Type.GetTypeInterface(typeof(IQueryTelegramCommand<>)) != null;

        public bool IsSessionTelegramCommand =>
            Type.GetTypeInterface(typeof(ISessionTelegramCommand<,>)) != null;

        public bool IsBehaviorTelegramCommand =>
            GetBehaviorInterfaceType() != null;

        public bool Authorized => Descriptor.Authorized;

        private Type GetBehaviorInterfaceType()
        {
            return Type.GetTypeInterface(typeof(IBehaviorTelegramCommand<>));
        }

        private Type GetSessionInterfaceType()
        {
            return Type.GetTypeInterface(typeof(ISessionTelegramCommand<,>));
        }

        public Type GetSessionObjectType()
        {
            if (IsBehaviorTelegramCommand)
            {
                return GetBehaviorSessionObjectType();
            }

            if (IsSessionTelegramCommand)
            {
                var comInterfaceType = GetSessionInterfaceType();
                var args = comInterfaceType
                    .GetGenericArguments();
                if (args.Length != 2)
                    throw new TelegramExtractionCommandInternalException("Is not a session command");
                return args[1];
            }

            throw new TelegramExtractionCommandInternalException("Is not a session command");
        }

        private Type GetBehaviorSessionObjectType()
        {
            var comInterfaceType = GetBehaviorInterfaceType();
            var args = comInterfaceType
                .GetGenericArguments();
            return args[0];
        }
    }
}