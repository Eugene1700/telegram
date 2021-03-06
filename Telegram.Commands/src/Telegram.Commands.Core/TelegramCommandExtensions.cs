﻿using System;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;

namespace Telegram.Commands.Core
{
    public static class TelegramCommandExtensions
    {
        public static string ExtractData<T>(this ITelegramCommand<T> command, T query)
        {
            var type = command.GetType();
            var attr = type.GetCustomAttribute<CommandAttribute>();
            if (attr == null)
                throw new InvalidOperationException("unknown command");
            var data = query.GetData();
            var com = "/" + TelegramQueryExtensions.ExtractCommand(data);
            return data.Replace(com, "").Trim();
        }

        public static long GetChatId(this CallbackQuery callbackQuery)
        {
            return callbackQuery.GetChatId<CallbackQuery>();
        }
        
        public static long GetChatId(this Message message)
        {
            return message.GetChatId<Message>();
        }

        public static ITelegramCommandDescriptor GetCommandInfo<TQuery>(this ITelegramCommand<TQuery> command)
        {
            return GetCommandInfo(command.GetType());
        }
        
        public static ITelegramCommandDescriptor GetCommandInfo<TCommand, TQuery>() where TCommand: ITelegramCommand<TQuery>
        {
            return GetCommandInfo(typeof(TCommand));
        }

        public static ITelegramCommandDescriptor GetCommandInfo(Type commandType)
        {
            return commandType?.GetCustomAttribute<CommandAttribute>();
        }
        
        public static string GetCommandQuery(this ITelegramCommandDescriptor commandInfo)
        {
            return $"/{commandInfo.Name}";
        }
        
        public static string GetCommandQuery<TCommand>() where TCommand : ITelegramCommand<Message>
        {
            return GetCommandInfo<TCommand, Message>().GetCommandQuery();
        }
    }
}