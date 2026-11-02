using System;
using System.Reflection;

using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent;

namespace Bot.Commands.Core
{
    public static class BotCommandExtensions
    {
        public static string ExtractData<TQuery>(this IQueryCommand<TQuery> command, QueryContext<TQuery> query)
        {
            var type = command.GetType();
            return ExtractData(query, type);
        }

        public static string ExtractData<TQuery, TSessionObject>(this ISessionCommand<TQuery, TSessionObject> command, QueryContext<TQuery> query)
        {
            var type = command.GetType();
            return ExtractData(query, type);
        }

        public static ICommandDescriptor GetCommandInfo<TQuery>(this IQueryCommand<TQuery> command)
        {
            return GetCommandInfo(command.GetType());
        }

        public static ICommandDescriptor GetCommandInfo<TObj, TStates>(this FluentCommand<TObj, TStates> command)
        {
            return GetCommandInfo(command.GetType());
        }

        public static ICommandDescriptor GetCommandInfo<TCommand, TQuery>()
            where TCommand: IQueryCommand<TQuery>
        {
            return GetCommandInfo(typeof(TCommand));
        }

        public static ICommandDescriptor GetBehaviorCommandInfo<TCommand, TSessionObject>()
            where TCommand: IBehaviorCommand<TSessionObject>
        {
            return GetCommandInfo(typeof(TCommand));
        }

        public static ICommandDescriptor GetFluentCommandInfo<TCommand, TObject, TStates>()
            where TCommand: FluentCommand<TObject, TStates>
        {
            return GetCommandInfo(typeof(TCommand));
        }

        public static ICommandDescriptor GetCommandInfo<TCommand, TQuery, TSessionObject>()
            where TCommand: ISessionCommand<TQuery, TSessionObject>
        {
            return GetCommandInfo(typeof(TCommand));
        }

        public static ICommandDescriptor GetCommandInfo(Type commandType)
        {
            return commandType?.GetCustomAttribute<CommandAttribute>();
        }

        public static string GetCommandQuery(this ICommandDescriptor commandInfo)
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
            var parts = data.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[1].Trim() : string.Empty;
        }

        public static string ExtractCommand(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return string.Empty;
            var token = query.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[0];
            if (token[0] == '/') token = token.Substring(1);
            return token;
        }

        public static bool MatchCommand(this ICommandDescriptor descriptor, string query)
        {
            if (string.IsNullOrEmpty(query)) return false;

            var com = ExtractCommand(query);
            return com == descriptor.Name;
        }

        public static bool MatchCommand(this ICommandDescriptor descriptor, ICommandDescriptor anotherDescriptor)
        {
            return descriptor.Name == anotherDescriptor.Name;
        }
    }
}
