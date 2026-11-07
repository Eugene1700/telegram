using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Services
{
    public class CommandExecutionService
    {
        private readonly ICommandFactory _commandFactory;
        private readonly SessionManager _sessionManager;

        public CommandExecutionService(ICommandFactory commandFactory,
            SessionManager sessionManager)
        {
            _commandFactory = commandFactory;
            _sessionManager = sessionManager;
        }

        public async Task<ICommandExecutionResult> Execute<TQuery>(
            CommandDescriptorComposition fullCommandDescriptor,
            IQueryContext<TQuery> query)
        {
            try
            {
                return await ExecuteInternal(fullCommandDescriptor, query);
            }
            catch (TargetInvocationException invocationException)
            {
                if (invocationException.InnerException != null) 
                    throw invocationException.InnerException;
                throw;
            }
        }

        private async Task<ICommandExecutionResult> ExecuteInternal<TQuery>(
            CommandDescriptorComposition fullCommandDescriptor,
            IQueryContext<TQuery> query)
        {
            if (fullCommandDescriptor.SessionCommand != null)
            {
                var commandType = fullCommandDescriptor.SessionCommand.Type;
                var sessionCommandInstance = _commandFactory.GetCommand(commandType);
                if (fullCommandDescriptor.SessionCommand.IsBehaviorCommand)
                {
                    MethodInfo executeMethod = null;
                    var queryCommandDesc = fullCommandDescriptor.QueryCommand;
                    if (queryCommandDesc != null)
                    {
                        var queryCommandInstance =
                            _commandFactory.GetCommand(queryCommandDesc.Type);
                        var behaviorCommandMethods = GetExecuteMethods(commandType);
                        
                        if (fullCommandDescriptor.QueryCommand?.IsSessionCommand ?? false)
                        {
                            executeMethod = behaviorCommandMethods
                                .Single(x =>
                                {
                                    var parType = x.GetParameters()[0].ParameterType;
                                    return parType.IsGenericType && parType.GetGenericTypeDefinition() ==
                                        typeof(ISessionCommand<,>);
                                });
                        }

                        if (fullCommandDescriptor.QueryCommand?.IsQueryCommand ?? false)
                        {
                            executeMethod = behaviorCommandMethods
                                .Single(x =>
                                {
                                    var parType = x.GetParameters()[0].ParameterType;
                                    return parType.IsGenericType && parType.GetGenericTypeDefinition() ==
                                        typeof(IQueryCommand<>);
                                });
                        }
                        
                        if (executeMethod == null)
                            throw new InvalidOperationException("Is not a bot command");
                        
                        var genericMethod = executeMethod.MakeGenericMethod(typeof(TQuery));
                        var behaviorSessionObject = GetSessionObject(fullCommandDescriptor, query.GetChatId(), query.GetFromId(), typeof(TQuery));
                        // ReSharper disable once PossibleNullReferenceException
                        return await (Task<ICommandExecutionResult>) genericMethod.Invoke(sessionCommandInstance,
                            new[] {queryCommandInstance, query, behaviorSessionObject});
                    }

                    if (fullCommandDescriptor.QueryCommand == null)
                    {
                        executeMethod = GetDefaultExecuteMethod(commandType);
                    }

                    if (executeMethod == null)
                        throw new InvalidOperationException("Is not a bot command");
                    
                    var genericMethodWithoutQueryComInstance = executeMethod.MakeGenericMethod(typeof(TQuery));
                    var behaviorSessionObjectWithoutQueryComInstance = GetSessionObject(fullCommandDescriptor, query.GetChatId(), query.GetFromId(), typeof(TQuery));
                    // ReSharper disable once PossibleNullReferenceException
                    return await (Task<ICommandExecutionResult>) genericMethodWithoutQueryComInstance.Invoke(sessionCommandInstance,
                        new[] {query, behaviorSessionObjectWithoutQueryComInstance});
                }

                var sessionCommandMethod = GetExecuteMethod<TQuery>(commandType);
                var sessionObject = GetSessionObject(fullCommandDescriptor, query.GetChatId(), query.GetFromId(), typeof(TQuery));
                // ReSharper disable once PossibleNullReferenceException
                return await (Task<ICommandExecutionResult>) sessionCommandMethod.Invoke(sessionCommandInstance,
                    new[] {query, sessionObject});
            }

            if (fullCommandDescriptor.QueryCommand != null)
            {
                if (fullCommandDescriptor.QueryCommand.IsBehaviorCommand)
                {
                    var behCommandType = fullCommandDescriptor.QueryCommand.Type;
                    var executeMethod = GetDefaultExecuteMethod(behCommandType);
                    var genericMethodWithoutQueryComInstance = executeMethod.MakeGenericMethod(typeof(TQuery));
                    var sessionCommandInstance = _commandFactory.GetCommand(behCommandType);
                    // ReSharper disable once PossibleNullReferenceException
                    return await (Task<ICommandExecutionResult>)genericMethodWithoutQueryComInstance.Invoke(
                        sessionCommandInstance,
                        new[] { query, (object)null });
                }
                var commandType = fullCommandDescriptor.QueryCommand.Type;
                var commandInstance = (IQueryCommand<TQuery>) _commandFactory.GetCommand(commandType);
                return await commandInstance.Execute(query);
            }

            throw new CommandExtractionException("Cann't execute command", query.GetChatId());
        }

        private object GetSessionObject(CommandDescriptorComposition fullCommandDescriptor, long chatId, long fromId, Type queryType)
        {
            var sessionObjectType = fullCommandDescriptor.SessionCommand.GetSessionObjectType(queryType);
            var sessionObject = _sessionManager.GetCurrentSession(chatId, fromId,
                fullCommandDescriptor.SessionCommand.Descriptor.GetCommandQuery(), sessionObjectType).Data;
            return sessionObject;
        }

        private static MethodInfo[] GetExecuteMethods(Type commandType)
        {
            var methods = commandType.GetTypeInterface(typeof(IBehaviorCommand<>))
                .GetMethods().Where(x => x.Name == "Execute").ToArray();
            if (methods == null || !methods.Any())
                throw new InvalidOperationException("Is not a bot command");
            return methods;
        }
        
        private static MethodInfo GetDefaultExecuteMethod(Type commandType)
        {
            return commandType.GetTypeInterface(typeof(IBehaviorCommand<>)).GetMethod("DefaultExecute");
        }

        private static MethodInfo GetExecuteMethod<TQuery>(Type commandType)
        {
            var method = commandType.GetInterfaces().Single(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(ISessionCommand<,>) &&
                i.GetGenericArguments()[0] == typeof(TQuery)).GetMethod("Execute");
            if (method == null)
                throw new InvalidOperationException("Is not a bot command");
            return method;
        }
    }
}
