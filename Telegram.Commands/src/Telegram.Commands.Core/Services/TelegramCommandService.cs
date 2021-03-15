using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Services
{
    internal sealed class TelegramCommandService : ITelegramCommandResolver
    {
        private readonly ITelegramBotClient _telegramClient;
        private readonly ITelegramCommandFactory _commandFactory;
        private readonly IAuthProvider _authProvider;
        private readonly SessionManager _sessionManager;
        private readonly ITelegramBotProfile _telegramBotProfile;

        public TelegramCommandService(ITelegramBotClient telegramClient,
            ITelegramCommandFactory commandFactory,
            IAuthProvider authProvider,
            SessionManager sessionManager,
            ITelegramBotProfile telegramBotProfile)
        {
            _telegramClient = telegramClient;
            _commandFactory = commandFactory;
            _authProvider = authProvider;
            _sessionManager = sessionManager;
            _telegramBotProfile = telegramBotProfile;
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
                var chatId = query.GetChatId();
                var userId = query.GetFromId();
                ITelegramCommandDescriptor commandDescriptor;
                ITelegramCommand<T> command;
                (commandDescriptor, command) = await GetCommand(query);
                if (command != null)
                {
                    var commandExecutionResult = await command.Execute(query);
                    if (commandExecutionResult.Result == ExecuteResult.Freeze)
                        return;

                    if (commandExecutionResult.Result == ExecuteResult.Break)
                    {
                        await _sessionManager.ReleaseSessionIfExists(chatId, userId);
                        return;
                    }

                    if (commandExecutionResult.Result == ExecuteResult.Ahead)
                    {
                        var sessionChatId = commandExecutionResult.WaitFromChatId ?? chatId;
                        switch (commandDescriptor.Chain)
                        {
                            case CommandChain.StartPoint:
                                await _sessionManager.OpenSession(commandExecutionResult.NextCommandDescriptor,
                                    sessionChatId,
                                    userId, commandExecutionResult.Data, commandExecutionResult.SessionDurationInSec);
                                return;
                            case CommandChain.TransitPoint:
                                await _sessionManager.ContinueSession(commandExecutionResult.NextCommandDescriptor,
                                    chatId,
                                    sessionChatId,
                                    userId, commandExecutionResult.Data, commandExecutionResult.SessionDurationInSec);
                                return;
                            case CommandChain.EndPoint:
                                await _sessionManager.ReleaseSessionIfExists(sessionChatId, userId);
                                return;
                            case CommandChain.None:
                                return;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            catch (TelegramException ex)
            {
                var message = query.AsMessage();
                await _telegramClient.SendTextMessageAsync(message.Chat.Id, ex.Message,
                    replyToMessageId: message.MessageId);
            }
        }

        private async Task<(ITelegramCommandDescriptor, ITelegramCommand<T>)> GetCommand<T>(T query)
        {
            var fromSession = false;
            if (TryGetSessionCommandStr(query, out var commandStr))
            {
                fromSession = true;
            }
            else if (!TryGetQueryCommandStr(query, out commandStr))
                if (query.IsGroupMessage())
                    return (null, null);
                else
                    throw new TelegramException("Could not extract command");

            var commandType = FindCommandByQuery<T>(commandStr);

            var commandInfo = TelegramCommandExtensions.GetCommandInfo(commandType);

            AssertChatType(query, commandInfo);

            switch (commandInfo.Permission)
            {
                case Permissions.Guest:
                    var command = await _commandFactory.GetCommand(query, commandType);
                    return (commandInfo, command);
                case Permissions.Callback:
                {
                    if (!QueryIsCallback(query))
                        //todo add onlycallback exception
                        throw new TelegramException($"This command only for callbackquery");
                    break;
                }
                case Permissions.Session:
                    if (!fromSession)
                        //todo add only session exception
                        throw new TelegramException("This command only for session");
                    break;
                default:
                {
                    var user = await _authProvider.AuthUser(query.GetFromId());
                    if (user == null)
                        //todo auth exception
                        throw new TelegramException("User not found");

                    if (user.Permission < commandInfo.Permission)
                        //todo permission exception
                        throw new TelegramException("You doesn't have permission for this command");
                    break;
                }
            }

            var com = await _commandFactory.GetCommand(query, commandType);
            return (commandInfo, com);
        }

        private void AssertChatType<T>(T query, ITelegramCommandDescriptor commandInfo)
        {
            if (commandInfo == null)
                throw new ArgumentNullException(nameof(commandInfo));
            var chatType = query.GetChatType();
            switch (chatType)
            {
                case ChatType.Private:
                    if ((commandInfo.Area & ChatArea.Private) != ChatArea.Private)
                        //todo chat area exception
                        throw new TelegramException("This command is not for private chat");
                    return;
                case ChatType.Group:
                    if ((commandInfo.Area & ChatArea.Group) != ChatArea.Group)
                        //todo chat area exception
                        throw new TelegramException("This command is not for group");
                    return;
                case ChatType.Channel:
                    if ((commandInfo.Area & ChatArea.Channel) != ChatArea.Channel)
                        //todo chat area exception
                        throw new TelegramException("This command is not for channel");
                    return;
                case ChatType.Supergroup:
                    //todo chat area exception
                    if ((commandInfo.Area & ChatArea.SuperGroup) != ChatArea.SuperGroup)
                        throw new TelegramException("This command is not for supergroup");
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private bool TryGetQueryCommandStr<T>(T query, out string commandStr)
        {
            commandStr = query.GetData();
            var isCommand = !string.IsNullOrWhiteSpace(commandStr) && commandStr[0] == '/';
            if (!isCommand)
                return false;
            if (!query.IsGroupMessage()) return true;
            var botName = _telegramBotProfile.BotName;
            return commandStr.EndsWith($"@{botName}");
        }

        private static Type FindCommandByQuery<T>(string queryString)
        {
            //todo need cache
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
}