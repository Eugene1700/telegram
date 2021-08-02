using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;

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
                    case UpdateType.MyChatMember:
                        await QueryHandler(update.MyChatMember);
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

        private async Task<(ITelegramCommandDescriptor, ITelegramCommand<T>)> GetEvent<T>(T query)
        {
            if (!IsEvent(query))
                return (null,null);

            Type type = null;
            if (query is ChatMemberUpdated)
            {
                type = FindEventByType(EventType.BotMemberAddedToChat);
            }

            if (query is Message mes)
            {
                switch (mes.Type)
                {
                    case MessageType.MigratedToSupergroup:
                    case MessageType.MigratedFromGroup:
                        type = FindEventByType(EventType.MigrateToSuperGroup);
                        break;
                    case MessageType.ChatMembersAdded:
                        type = FindEventByType(EventType.ChatMemberAdded);
                        break;
                    default:
                        return (null,null);
                }
            }
            
            var info = TelegramCommandExtensions.GetCommandInfo(type);
            return (info, await GetCommandInstance(query, info, type, false));
        }

        private async Task QueryHandler<T>(T query)
        {
            try
            {
                var chatId = query.GetChatId();
                var userId = query.GetFromId();
                var (commandInfo,command) = await GetCommand(query);
                // if (command is ISwarmVisa swarmPassport)
                // {
                //     if (_telegramBotProfile is ISwarmVisa profileSwarmVisa)
                //     {
                //         if (!swarmPassport.MySwarm.SameSwarm(profileSwarmVisa.MySwarm))
                //             throw new TelegramExtractionCommandException("Could not extract command", chatId);
                //     }
                //     else
                //     {
                //         throw new TelegramExtractionCommandException("Could not extract command", chatId);
                //     }
                // }
                if (command != null)
                {
                    var commandExecutionResult = await command.Execute(query);
                    if (commandExecutionResult.Result == ExecuteResult.Freeze)
                        return;

                    var sessionChatId = commandExecutionResult.WaitFromChatId ?? chatId;
                    if (commandExecutionResult.Result == ExecuteResult.Break)
                    {
                        await _sessionManager.ReleaseSessionIfExists(sessionChatId, userId);
                        return;
                    }

                    var activeSession = _sessionManager.GetCurrentSession(chatId, userId);
                    if (commandExecutionResult.Result == ExecuteResult.Ahead)
                    {
                        if (activeSession == null)
                        {
                            await _sessionManager.OpenSession(commandExecutionResult.NextCommandDescriptor,
                                sessionChatId,
                                userId, commandExecutionResult.Data, commandExecutionResult.SessionDurationInSec);
                            return;
                        }
                        
                        await _sessionManager.ContinueSession(commandExecutionResult.NextCommandDescriptor,
                            chatId,
                            sessionChatId,
                            userId, commandExecutionResult.Data, commandExecutionResult.SessionDurationInSec);

                        return;
                    }

                    throw new ArgumentOutOfRangeException(nameof(commandExecutionResult.Result),
                        commandExecutionResult.Result.ToString());
                }
            }
            catch (TelegramDomainException ex)
            {
                var message = query.AsMessage();
                await _telegramClient.SendTextMessageAsync(message.Chat.Id, ex.Message,
                    replyToMessageId: message.MessageId);
            }
        }

        private async Task<(ITelegramCommandDescriptor,ITelegramCommand<T>)> GetCommand<T>(T query)
        {
            var chatId = query.GetChatId();
            var fromSession = false;
            if (TryGetSessionCommandStr(query, out var commandStr))
            {
                fromSession = true;
            }
            else if (IsEvent(query))
            {
                return await GetEvent(query);
            }
            else if (!TryGetQueryCommandStr(query, out commandStr))
                if (query.IsGroupMessage())
                    return (null,null);
                else
                    throw new TelegramExtractionCommandException("Could not extract command", chatId);
            
            var (commandInfo, commandType) = GetCommandInfo(query, commandStr, chatId, fromSession);
            return (commandInfo,await GetCommandInstance(query, commandInfo, commandType, fromSession));
        }

        private static bool IsEvent<T>(T query)
        {
            switch (query)
            {
                case ChatMemberUpdated _:
                    return true;
                case Message mes:
                    switch (mes.Type)
                    {
                        case MessageType.MigratedToSupergroup:
                        case MessageType.MigratedFromGroup:
                        case MessageType.ChatMembersAdded:
                            return true;
                        default:
                            return false;
                    }
                    default:
                        return false;
            }
        }

        private async Task<ITelegramCommand<T>> GetCommandInstance<T>(T query, 
            ITelegramCommandDescriptor commandInfo, Type commandType,
            bool fromSession)
        {
            var chatId = query.GetChatId();
            AssertChatType(query, commandInfo);

            switch (commandInfo.Permission)
            {
                case Permissions.Guest:
                    var command = _commandFactory.GetCommand(query, commandType);
                    return command;
                case Permissions.Callback:
                {
                    if (!QueryIsCallback(query))
                        throw new TelegramCommandsPermissionException($"This command only for callbackquery", chatId);
                    break;
                }
                case Permissions.Session:
                    if (!fromSession)
                        throw new TelegramCommandsPermissionException("This command only for session", chatId);
                    break;
                default:
                {
                    var user = await _authProvider.AuthUser(query.GetFromId());
                    if (user == null)
                        throw new TelegramCommandsPermissionException("User not found", chatId);

                    if (user.Permission < commandInfo.Permission)
                        throw new TelegramCommandsPermissionException("You don't have permission for this command", chatId);
                    break;
                }
            }

            var com = _commandFactory.GetCommand(query, commandType);
            return com;
        }

        private (ITelegramCommandDescriptor,Type) GetCommandInfo<T>(T query, string commandStr, long chatId, bool fromSession)
        {
            var commandTypes = FindCommandByQuery(commandStr);
            var (commandInfo, commandType) = ApplySwarm(chatId, commandTypes);
            if (commandInfo == null)
            {
                throw new TelegramExtractionCommandException("Command without attribute", chatId);
            }

            if (!fromSession || !TryGetQueryCommandStr(query, out var currentCommandStr))
                return (commandInfo, commandType);
            
            var currentCommandTypes = FindCommandByQuery(currentCommandStr);
            var (currentCommandInfo, currentCommandType) = ApplySwarm(chatId, currentCommandTypes);
            if (currentCommandInfo == null)
            {
                throw new TelegramExtractionCommandException("Command without attribute", chatId);
            }

            if (commandInfo.MatchReaction(currentCommandInfo))
            {
                return (currentCommandInfo, currentCommandType);
            }

            return (commandInfo,commandType);
        }

        private static void AssertChatType<T>(T query, ITelegramCommandDescriptor commandInfo)
        {
            if (commandInfo == null)
                throw new ArgumentNullException(nameof(commandInfo));
            var chatType = query.GetChatType();
            var chatId = query.GetChatId();
            switch (chatType)
            {
                case ChatType.Private:
                    if ((commandInfo.Area & ChatArea.Private) != ChatArea.Private)
                        throw new TelegramCommandsChatAreaException("This command is not for private chat", chatId);
                    return;
                case ChatType.Group:
                    if ((commandInfo.Area & ChatArea.Group) != ChatArea.Group)
                        throw new TelegramCommandsChatAreaException("This command is not for group", chatId);
                    return;
                case ChatType.Channel:
                    if ((commandInfo.Area & ChatArea.Channel) != ChatArea.Channel)
                        throw new TelegramCommandsChatAreaException("This command is not for channel", chatId);
                    return;
                case ChatType.Supergroup:
                    if ((commandInfo.Area & ChatArea.SuperGroup) != ChatArea.SuperGroup)
                        throw new TelegramCommandsChatAreaException("This command is not for supergroup", chatId);
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

        private static Type[] FindCommandByQuery(string queryString)
        {
            //todo need cache
            // var comType = typeof(ITelegramCommand<T>);
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p =>
                {
                    var attrLoc = p.GetCustomAttribute<CommandAttribute>();
                    return p.IsClass && !p.IsAbstract && attrLoc != null && attrLoc.MatchCommand(queryString);
                }).ToArray();

            return commandTypes;
        }

        private (ITelegramCommandDescriptor, Type) ApplySwarm(long chatId, Type[] suspectedTypes)
        {
            if (_telegramBotProfile is ISwarmVisa swarmVisa)
            {
                var descs = suspectedTypes.Select(x=> (TelegramCommandExtensions.GetCommandInfo(x), x))
                    .Where(x => x.Item1.Swarms == null || x.Item1.Swarms.Any(y => y == swarmVisa.MySwarm)).ToArray();
                if (descs.Length == 1)
                    return descs[0];
                
                throw new TelegramExtractionCommandException("Too many commands for query", chatId);
            }

            if (suspectedTypes.Length == 1)
                return (TelegramCommandExtensions.GetCommandInfo(suspectedTypes[0]), suspectedTypes[0]);

            throw new TelegramExtractionCommandException("Too many commands for query", chatId);
        }
        
        private static Type FindEventByType(EventType eventType)
        {
            //todo need cache
            var commandType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .SingleOrDefault(p =>
                {
                    var attrLoc = p.GetCustomAttribute<EventAttribute>();
                    return p.IsClass && !p.IsAbstract && attrLoc != null && attrLoc.Type == eventType;
                });
            
            return commandType;
        }
    }
}