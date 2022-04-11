using System;
using System.Linq;
using System.Reflection;
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

        public bool IsBaseCommand =>
            Type.GetTypeInterface(typeof(IBaseCommand<,,>)) != null;

        public bool IsBehaviorCommand =>
            GetBehaviorInterfaceType() != null;

        public bool Authorized => Descriptor.Authorized;

        private Type GetBehaviorInterfaceType()
        {
            return Type.GetTypeInterface(typeof(IBehaviorTelegramCommand<,>));
        }

        private Type GetBaseInterfaceType()
        {
            return Type.GetTypeInterface(typeof(IBaseCommand<,,>));
        }

        public Type GetSessionObjectType()
        {
            if (IsBehaviorCommand)
            {
                return GetBehaviorSessionObjectType();
            }

            if (IsBaseCommand)
            {
                var comInterfaceType = GetBaseInterfaceType();
                var args = comInterfaceType
                    .GetGenericArguments();
                //todo Третитий аргумент не должен быть EmptyObject
                if (args.Length != 3)
                    throw new TelegramExtractionCommandInternalException("Is not a session command");
                return args[2];
            }

            throw new TelegramExtractionCommandInternalException("Is not a session command");
        }
        
        public Type GetQueryObjectType()
        {
            if (IsBehaviorCommand)
            {
                return GetBehaviorSessionObjectType();
            }

            if (IsBaseCommand)
            {
                var comInterfaceType = GetBaseInterfaceType();
                var args = comInterfaceType
                    .GetGenericArguments();
                //todo Второй аргумент не должен быть EmptyObject
                if (args.Length != 3)
                    throw new TelegramExtractionCommandInternalException("Is not a queryData command");
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