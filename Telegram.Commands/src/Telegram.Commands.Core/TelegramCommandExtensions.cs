using System;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core
{
    public static class TelegramCommandExtensions
    {
        public static string ExtractData<T>(this IQueryTelegramCommand<T> command, T query)
        {
            var type = command.GetType();
            return ExtractData(query, type);
        }

        public static string ExtractData<TQuery, TSessionObject>(this ISessionTelegramCommand<TQuery, TSessionObject> command, TQuery query)
        {
            var type = command.GetType();
            return ExtractData(query, type);
        }

        public static long GetChatId(this CallbackQuery callbackQuery)
        {
            return callbackQuery.GetChatId<CallbackQuery>();
        }
        
        public static long GetChatId(this Message message)
        {
            return message.GetChatId<Message>();
        }

        public static ITelegramCommandDescriptor GetCommandInfo<TQuery>(this IQueryTelegramCommand<TQuery> command)
        {
            return GetCommandInfo(command.GetType());
        }
        
        public static ITelegramCommandDescriptor GetCommandInfo<TObj>(this FluentCommand<TObj> command)
        {
            return GetCommandInfo(command.GetType());
        }

        public static ITelegramCommandDescriptor GetCommandInfo<TCommand, TQuery>() 
            where TCommand: IQueryTelegramCommand<TQuery>
        {
            return GetCommandInfo(typeof(TCommand));
        }
        
        public static ITelegramCommandDescriptor GetBehaviorCommandInfo<TCommand, TSessionObject>() 
            where TCommand: IBehaviorTelegramCommand<TSessionObject>
        {
            return GetCommandInfo(typeof(TCommand));
        }
        
        public static ITelegramCommandDescriptor GetFluentCommandInfo<TCommand, TObject>() 
            where TCommand: FluentCommand<TObject>
        {
            return GetCommandInfo(typeof(TCommand));
        }
        
        public static ITelegramCommandDescriptor GetCommandInfo<TCommand, TQuery, TSessionObject>() 
            where TCommand: ISessionTelegramCommand<TQuery, TSessionObject>
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
        
        public static string GetCommandQuery<TCommand>() where TCommand : IQueryTelegramCommand<Message>
        {
            return GetCommandInfo<TCommand, Message>().GetCommandQuery();
        }
        
        public static string ExtractData<T>(T query, Type type)
        {
            var attr = type.GetCustomAttribute<CommandAttribute>();
            if (attr == null)
                throw new InvalidOperationException("unknown command");
            var data = query.GetData();
            var com = "/" + TelegramQueryExtensions.ExtractCommand(data);
            return data.Replace(com, "").Trim();
        }
    }
}