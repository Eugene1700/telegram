using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

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

        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(long chatId, long userId,
            CommandDescriptorComposition fullCommandDescriptor, TQuery query)
        {
            if (fullCommandDescriptor.SessionCommand != null)
            {
                if (fullCommandDescriptor.SessionCommand.IsBehaviorTelegramCommand)
                {
                    var commandType = fullCommandDescriptor.SessionCommand.Type;
                    var behaviorInstance =
                        _telegramCommandFactory.GetCommand(commandType);
                    var queryCommandInstance =
                        _telegramCommandFactory.GetCommand(fullCommandDescriptor.QueryCommand.Type);

                    var method = commandType.GetMethod("Execute");
                    if (method == null)
                        throw new InvalidOperationException("Is not a telegram command");
                    
                    var sessionObject = _sessionManager.GetCurrentSession(chatId, userId, args[1]).Data;
                    // ReSharper disable once PossibleNullReferenceException
                    return await (Task<ITelegramCommandExecutionResult>) method.Invoke(behaviorInstance,
                        new[] {queryCommandInstance, query, sessionObject});
                }
            }
        }

        private async Task<ITelegramCommandExecutionResult> InvokeSessionTelegramMethod(object query,
            long chatId, long userId,
            Type commandType, object command)
        {
            var args = GetSessionCommandInterfaceType(commandType).GetGenericArguments();
            if (args.Length != 2)
                throw new InvalidOperationException();
            var sessionObject = _sessionManager.GetCurrentSession(chatId, userId, args[1]).Data;

            if (method == null)
                throw new InvalidOperationException("Is not a telegram command");
            // ReSharper disable once PossibleNullReferenceException
            return await (Task<ITelegramCommandExecutionResult>) method.Invoke(command, new[] {query, sessionObject});
        }
    }
}