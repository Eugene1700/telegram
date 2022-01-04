using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Services
{
    internal class CommandExecutionService
    {
        private readonly ITelegramCommandFactory _telegramCommandFactory;
        private readonly SessionManager _sessionManager;

        public CommandExecutionService(ITelegramCommandFactory telegramCommandFactory,
            SessionManager sessionManager)
        {
            _telegramCommandFactory = telegramCommandFactory;
            _sessionManager = sessionManager;
        }

        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(
            CommandDescriptorComposition fullCommandDescriptor,
            TQuery query)
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

        private async Task<ITelegramCommandExecutionResult> ExecuteInternal<TQuery>(
            CommandDescriptorComposition fullCommandDescriptor,
            TQuery query)
        {
            var chatId = query.GetChatId();
            var fromId = query.GetFromId();
            if (fullCommandDescriptor.SessionCommand != null)
            {
                var commandType = fullCommandDescriptor.SessionCommand.Type;
                var sessionCommandInstance = _telegramCommandFactory.GetCommand(commandType);
                if (fullCommandDescriptor.SessionCommand.IsBehaviorTelegramCommand)
                {
                    MethodInfo executeMethod = null;
                    var queryCommandDesc = fullCommandDescriptor.QueryCommand;
                    if (queryCommandDesc != null)
                    {
                        var queryCommandInstance =
                            _telegramCommandFactory.GetCommand(queryCommandDesc.Type);
                        var behaviorCommandMethods = GetExecuteMethods(commandType);
                        
                        if (fullCommandDescriptor.QueryCommand?.IsSessionTelegramCommand ?? false)
                        {
                            executeMethod = behaviorCommandMethods
                                .Single(x =>
                                {
                                    var parType = x.GetParameters()[0].ParameterType;
                                    return parType.IsGenericType && parType.GetGenericTypeDefinition() ==
                                        typeof(ISessionTelegramCommand<,>);
                                });
                        }

                        if (fullCommandDescriptor.QueryCommand?.IsQueryCommand ?? false)
                        {
                            executeMethod = behaviorCommandMethods
                                .Single(x =>
                                {
                                    var parType = x.GetParameters()[0].ParameterType;
                                    return parType.IsGenericType && parType.GetGenericTypeDefinition() ==
                                        typeof(IQueryTelegramCommand<>);
                                });
                        }
                        
                        if (executeMethod == null)
                            throw new InvalidOperationException("Is not a telegram command");
                        
                        var genericMethod = executeMethod.MakeGenericMethod(typeof(TQuery));
                        var behaviorSessionObject = GetSessionObject(fullCommandDescriptor, chatId, fromId);
                        // ReSharper disable once PossibleNullReferenceException
                        return await (Task<ITelegramCommandExecutionResult>) genericMethod.Invoke(sessionCommandInstance,
                            new[] {queryCommandInstance, query, behaviorSessionObject});
                    }

                    if (fullCommandDescriptor.QueryCommand == null)
                    {
                        executeMethod = GetDefaultExecuteMethod(commandType);
                    }

                    if (executeMethod == null)
                        throw new InvalidOperationException("Is not a telegram command");
                    
                    var genericMethodWithoutQueryComInstance = executeMethod.MakeGenericMethod(typeof(TQuery));
                    var behaviorSessionObjectWithoutQueryComInstance = GetSessionObject(fullCommandDescriptor, chatId, fromId);
                    // ReSharper disable once PossibleNullReferenceException
                    return await (Task<ITelegramCommandExecutionResult>) genericMethodWithoutQueryComInstance.Invoke(sessionCommandInstance,
                        new[] {query, behaviorSessionObjectWithoutQueryComInstance});
                }

                var sessionCommandMethod = GetExecuteMethod(commandType);
                var sessionObject = GetSessionObject(fullCommandDescriptor, chatId, fromId);
                // ReSharper disable once PossibleNullReferenceException
                return await (Task<ITelegramCommandExecutionResult>) sessionCommandMethod.Invoke(sessionCommandInstance,
                    new[] {query, sessionObject});
            }

            if (fullCommandDescriptor.QueryCommand != null)
            {
                var commandType = fullCommandDescriptor.QueryCommand.Type;
                var commandInstance = (IQueryTelegramCommand<TQuery>) _telegramCommandFactory.GetCommand(commandType);
                return await commandInstance.Execute(query);
            }

            throw new TelegramExtractionCommandException("Cann't execute command", chatId);
        }

        private object GetSessionObject(CommandDescriptorComposition fullCommandDescriptor, long chatId, long fromId)
        {
            var sessionObjectType = fullCommandDescriptor.SessionCommand.GetSessionObjectType();
            var sessionObject = _sessionManager.GetCurrentSession(chatId, fromId, sessionObjectType).Data;
            return sessionObject;
        }

        private static MethodInfo[] GetExecuteMethods(Type commandType)
        {
            var methods = commandType.GetMethods().Where(x => x.Name == "Execute").ToArray();
            if (methods == null || !methods.Any())
                throw new InvalidOperationException("Is not a telegram command");
            return methods;
        }
        
        private static MethodInfo GetDefaultExecuteMethod(Type commandType)
        {
            return commandType.GetMethod("DefaultExecute");
        }

        private static MethodInfo GetExecuteMethod(Type commandType)
        {
            var method = commandType.GetMethod("Execute");
            if (method == null)
                throw new InvalidOperationException("Is not a telegram command");
            return method;
        }
    }
}