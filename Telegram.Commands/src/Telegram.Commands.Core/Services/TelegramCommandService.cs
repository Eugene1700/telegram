using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Services
{
    public class TelegramCommandService
    {
        private readonly TelegramClient _telegramClient;
        private readonly ITelegramCommandFactory _commandFactory;
        private readonly IAuthProvider _authProvider;
        private readonly ISessionManager _sessionManager;

        public TelegramCommandService(TelegramClient telegramClient,
            ITelegramCommandFactory commandFactory, 
            IAuthProvider authProvider,
            ISessionManager sessionManager)
        {
            _telegramClient = telegramClient;
            _commandFactory = commandFactory;
            _authProvider = authProvider;
            _sessionManager = sessionManager;
        }

        public async Task<TelegramResult> Handle(Update update)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await QueryHandler(update.Message);
                        break;
                    case UpdateType.CallbackQuery:
                        await QueryHandler(update.CallbackQuery);
                        break;
                    case UpdateType.PreCheckoutQuery:
                        await QueryHandler(update.PreCheckoutQuery);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return new TelegramResult(true);
            }
            catch (NotImplementedException ex)
            {
                await _telegramClient.SendTextMessageAsync(update.GetChatId(), ex.Message);
                return new TelegramResult(false);
            }
        }

        private async Task QueryHandler<T>(T query)
        {
            try
            {
                var command = await GetCommand(query);
                if (command != null)
                {
                    await command.Execute(query);
                }
            }
            catch (TelegramException ex)
            {
                var message = query.AsMessage();
                await _telegramClient.SendTextMessageAsync(message.Chat.Id, ex.Message,
                    replyToMessageId: message.MessageId);
            }
        }

        private async Task<ITelegramCommand<T>> GetCommand<T>(T query)
        {
            if (!TryGetSessionCommandStr(query, out var commandStr ))
                if (!TryGetQueryCommandStr(query, out commandStr))
                    throw new InvalidOperationException("Wrong state");

            var commandType = FindCommandByQuery<T>(commandStr);

            var commandInfo = TelegramCommandExtensions.GetCommandInfo(commandType);

            switch (commandInfo.Permission)
            {
                case Permissions.Guest:
                    return await _commandFactory.GetCommand(query, commandType);
                case Permissions.Callback:
                {
                    if (!QueryIsCallback(query))
                        throw new TargetException($"This command only for callbackquery");
                    break;
                }
                default:
                {
                    var user = await _authProvider.AuthUser(query.GetFromId());
                    if (user == null)
                        throw new TelegramException("User not found");

                    if (user.Permission < commandInfo.Permission)
                        throw new TelegramException("You doesn't have permission for this command");
                    break;
                }
            }

            return await _commandFactory.GetCommand(query, commandType);
        }

        private static bool QueryIsCallback<T>(T query)
        {
            if (query is CallbackQuery || query is PreCheckoutQuery)
                return true;
            var successfulPayment = query as Message;
            return successfulPayment?.SuccessfulPayment != null;
        }

        private bool TryGetSessionCommandStr<T>(T query, out string commandStr)
        {
            commandStr = null;
            var chatId = query.GetChatId();
            var userId = query.GetFromId();
            var sessionInfo = _sessionManager.GetCurrentSession(chatId, userId);
            if (sessionInfo == null) return false;
            commandStr = sessionInfo.CommandQuery;
            return true;

        }

        private static bool TryGetQueryCommandStr<T>(T query, out string commandStr)
        {
             commandStr = query.GetData();
             return commandStr[0] == '/';
        }

        private static Type FindCommandByQuery<T>(string queryString)
        {
            var comType = typeof(ITelegramCommand<T>);
            var commandType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .SingleOrDefault(p =>
                {
                    var attrLoc = p.GetCustomAttribute<CommandAttribute>();
                    return p.IsClass && !p.IsAbstract &&
                           comType.IsAssignableFrom(p) && attrLoc != null && attrLoc.MatchCommand(queryString);
                });
            return commandType;
        }
    }

    public class TelegramResult
    {
        public bool Ok { get; }

        public TelegramResult(bool ok)
        {
            Ok = ok;
        }
    }
}