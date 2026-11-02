using System;
using Bot.Commands.Abstract.Commands;
using System.Linq;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Core.Models
{
    public class FullCommandDescriptor
    {
        public FullCommandDescriptor(Type type)
        {
            Type = type;
            Descriptor = BotCommandExtensions.GetCommandInfo(type);
        }

        public ICommandDescriptor Descriptor { get; }
        public Type Type { get; }

        public bool IsQueryCommand => Type.GetInterfaces().Any(i => i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IQueryCommand<>));

        public bool IsSessionCommand =>
            Type.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(ISessionCommand<,>));

        public bool IsBehaviorCommand =>
            GetBehaviorInterfaceType() != null;

        public bool Authorized => Descriptor.Authorized;

        public bool SupportsQuery(Type queryType)
        {
            return IsBehaviorCommand || Type.GetInterfaces().Any(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IQueryCommand<>) ||
                 i.GetGenericTypeDefinition() == typeof(ISessionCommand<,>)) &&
                i.GetGenericArguments()[0] == queryType);
        }

        private Type GetBehaviorInterfaceType()
        {
            return Type.GetTypeInterface(typeof(IBehaviorCommand<>));
        }

        public Type GetSessionObjectType(Type queryType = null)
        {
            if (IsBehaviorCommand)
            {
                return GetBehaviorSessionObjectType();
            }

            if (IsSessionCommand)
            {
                return Type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISessionCommand<,>) &&
                        (queryType == null || i.GetGenericArguments()[0] == queryType))
                    .Select(i => i.GetGenericArguments()[1]).Distinct().Single();
            }

            throw new CommandExtractionInternalException("Is not a session command");
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
