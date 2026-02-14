using System;
using System.Reflection;
// using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent;

namespace Telegram.Commands.Core
{
    public static class BotCommandExtensions
    {
        public static string ExtractData<TQuery>(this IQueryTelegramCommand<TQuery> command, QueryContext<TQuery> query)
        {
            var type = command.GetType();
            return ExtractData(query, type);
        }
        
        public static string ExtractData<TQuery, TSessionObject>(this ISessionTelegramCommand<TQuery, TSessionObject> command, QueryContext<TQuery> query)
        {
            var type = command.GetType();
            return ExtractData(query, type);
        }

        public static ITelegramCommandDescriptor GetCommandInfo<TQuery>(this IQueryTelegramCommand<TQuery> command)
        {
            return GetCommandInfo(command.GetType());
        }
        
        public static ITelegramCommandDescriptor GetCommandInfo<TObj, TStates>(this FluentCommand<TObj, TStates> command)
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
        
        public static ITelegramCommandDescriptor GetFluentCommandInfo<TCommand, TObject, TStates>() 
            where TCommand: FluentCommand<TObject, TStates>
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
        
        public static string ExtractData<TQuery>(this QueryContext<TQuery> query, Type type)
        {
            var attr = type.GetCustomAttribute<CommandAttribute>();
            if (attr == null)
                throw new InvalidOperationException("unknown command");
            var data = query.GetData();
            if (data == null)
                return null;
            var com = "/" + ExtractCommand(data);
            return data.Replace(com, "").Trim();
        }
        
        public static string ExtractCommand(string query)
        {
            var com = query;
            if (query[0] == '/')
            {
                var botNameStart = query.IndexOf("@");
                var len = botNameStart == -1 ? query.Length - 1 : botNameStart - 1;
                com = query.Substring(1, len).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            return com;
        }
        
        public static bool MatchCommand(this ITelegramCommandDescriptor descriptor, string query)
        {
            if (string.IsNullOrEmpty(query)) return false;

            var com = ExtractCommand(query);
            return com == descriptor.Name;
        }
        
        public static bool MatchCommand(this ITelegramCommandDescriptor descriptor, ITelegramCommandDescriptor anotherDescriptor)
        {
            return descriptor.Name == anotherDescriptor.Name;
        }
    }
}