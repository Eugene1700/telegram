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
using Telegram.Commands.Core.Models;

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

        private async Task<GetCommandResult> GetEvent<T>(T query)
        {
            if (!IsEvent(query))
                return null;

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
                        return null;
                }
            }

            var info = TelegramCommandExtensions.GetCommandInfo(type);
            AssertChatType(query, info);
            ValidateQuery(query, type, false);
            return (info, await GetCommandInstance(query, info, type, false));
        }

        private async Task QueryHandler<T>(T query)
        {
            try
            {
                var chatId = query.GetChatId();
                var userId = query.GetFromId();
                var (_, command) = await GetCommand(query);
                if (command != null)
                {
                    var commandType = command.GetType();
                    var isSessionCommand = GetSessionCommandInterfaceType(commandType) != null;
                    var isTelegramCommand = commandType
                        .GetInterfaces()
                        .Any(i => i.IsGenericType &&
                                  i.GetGenericTypeDefinition() == typeof(ITelegramCommand<>));
                    ITelegramCommandExecutionResult commandExecutionResult;
                    if (isSessionCommand)
                    {
                        commandExecutionResult =
                            await InvokeSessionTelegramMethod(query, chatId, userId, commandType, command);
                    }
                    else if (isTelegramCommand)
                    {
                        commandExecutionResult = await InvokeTelegramMethod(query, commandType, command);
                    }
                    else
                    {
                        throw new InvalidOperationException("Didn't invoke the telegram command method");
                    }

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

        private static Type GetSessionCommandInterfaceType(Type commandType)
        {
            return commandType
                .GetInterfaces()
                .SingleOrDefault(i => i.IsGenericType &&
                                      i.GetGenericTypeDefinition() == typeof(ISessionTelegramCommand<,>));
        }

        private static async Task<ITelegramCommandExecutionResult> InvokeTelegramMethod(object query, Type commandType,
            object command)
        {
            var method = commandType.GetMethod("Execute");
            if (method == null)
                throw new InvalidOperationException("Is not a telegram command");
            return
                // ReSharper disable once PossibleNullReferenceException
                await (Task<ITelegramCommandExecutionResult>) method.Invoke(command, new[] {query});
        }

        private async Task<GetCommandResult> GetCommand<T>(T query)
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
                    return null;
                else
                    throw new TelegramExtractionCommandException("Could not extract command", chatId);

            var (commandInfo, commandType) = GetCommandInfo(query, commandStr, chatId, fromSession);
            AssertChatType(query, commandInfo);
            ValidateQuery(query, commandType, false);
            return (commandInfo, await GetCommandInstance(query, commandInfo, commandType, fromSession));
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

        private void ValidateQuery<TQuery>(TQuery query, Type commandType, bool fromSession)
        {
            var chatId = query.GetChatId();

            var commandInterfaceType = GetBehaviorInterfaceType(commandType);
            if (commandInterfaceType == null)
            {
                commandInterfaceType = GetSessionCommandInterfaceType(commandType);
            }

            if (commandInterfaceType != null && !fromSession)
            {
                throw new TelegramCommandsPermissionException("This command only for session", chatId);
            }

            if (commandInterfaceType == null)
            {
                commandInterfaceType = GetTelegramCommandInterfaceType(commandType);
            }

            if (commandInterfaceType != null)
            {
                var waitQueryType = commandInterfaceType.GenericTypeArguments[0];
                if (waitQueryType != query.GetType())
                {
                    throw new TelegramCommandsPermissionException(
                        $"Incompatible type for query and command wait {waitQueryType.Name}, but was {query.GetType().Name}",
                        chatId);
                }

                return;
            }

            throw new TelegramCommandsPermissionException("Unknown command", chatId);
        }

        private GetCommandResult GetCommandInfo<T>(T query, string commandName, long chatId,
            bool fromSession)
        {
            var commandTypes = FindCommandByName(commandName);
            var comDesc = ApplySwarm(chatId, commandTypes);
            if (comDesc == null)
            {
                throw new TelegramExtractionCommandException("Command without attribute", chatId);
            }

            if (!fromSession || !TryGetQueryCommandStr(query, out var currentCommandStr))
                return GetCommandResult.CreateSessionResult();

            var currentCommandTypes = FindCommandByName(currentCommandStr);
            var (currentCommandInfo, currentCommandType) = ApplySwarm(chatId, currentCommandTypes);
            if (currentCommandInfo == null)
            {
                throw new TelegramExtractionCommandException("Command without attribute", chatId);
            }

            return (commandInfo, commandType);
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

        private static Type[] FindCommandByName(string queryString)
        {
            //todo need cache
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p =>
                {
                    var attrLoc = p.GetCustomAttribute<CommandAttribute>();
                    return p.IsClass && !p.IsAbstract && attrLoc != null && attrLoc.MatchCommand(queryString);
                }).ToArray();

            return commandTypes;
        }

        private CommandTypeWithDescriptor ApplySwarm(long chatId, Type[] suspectedTypes)
        {
            var swarms = _telegramBotProfile.Swarms;
            if (_telegramBotProfile.Swarms != null && (_telegramBotProfile?.Swarms.Any() ?? false))
            {
                var descs = suspectedTypes.Select(x => new CommandTypeWithDescriptor
                    {
                        Descriptor = TelegramCommandExtensions.GetCommandInfo(x),
                        Type = x
                    })
                    .Where(x =>
                        x.Descriptor.Swarms == null || x.Descriptor.Swarms.Any(y => swarms.Contains(y))).ToArray();
                if (descs.Length == 1)
                    return descs[0];

                throw new TelegramExtractionCommandException("Too many commands for query", chatId);
            }

            if (suspectedTypes.Length == 1)
                return new CommandTypeWithDescriptor
                {
                    Descriptor = TelegramCommandExtensions.GetCommandInfo(suspectedTypes[0]),
                    Type = suspectedTypes[0]
                };

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

    public class FullCommandDescriptor
    {
        public FullCommandDescriptor(Type type)
        {
            Type = type;
            Descriptor = TelegramCommandExtensions.GetCommandInfo(type);
        }
        public ITelegramCommandDescriptor Descriptor { get; }
        public Type Type { get;}

        public bool IsTelegramCommand => GetTypeInterface(typeof(ITelegramCommand<>)) != null;

        private Type GetTypeInterface(Type interfaceType)
        {
            return Type
                .GetInterfaces()
                .SingleOrDefault(i => i.IsGenericType &&
                                      i.GetGenericTypeDefinition() == interfaceType);
        }

        public bool IsSessionTelegramCommand =>
            GetTypeInterface(typeof(ISessionTelegramCommand<,>)) != null;
        
        public bool IsBehaviorTelegramCommand => 
            GetBehaviorInterfaceType() != null;

        public Type GetBehaviorInterfaceType()
        {
            return GetTypeInterface(typeof(IBehaviorCommand<>));
        }
        
        public Type GetSessionObjectType()
        {
            var args = GetBehaviorInterfaceType()
                .GetGenericArguments();
            if (args.Length != 2)
                throw new InvalidOperationException();
            return args[1];
        }
        
        public 
    }

    internal class CommandDescriptorComposition
    {
        private CommandDescriptorComposition()
        {
        }

        public FullCommandDescriptor SessionCommand { get; set; }
        public FullCommandDescriptor QueryCommand { get; set; }

        public static CommandDescriptorComposition CreateSessionResult(FullCommandDescriptor sessionCommandDescriptor)
        {
            return new CommandDescriptorComposition
            {
                SessionCommand = sessionCommandDescriptor,
                QueryCommand = null
            };
        }
        
        public static CommandDescriptorComposition CreateBehaviorResult(FullCommandDescriptor sessionCommandDescriptor, 
            FullCommandDescriptor queryCommandDescriptor)
        {
            return new CommandDescriptorComposition
            {
                SessionCommand = sessionCommandDescriptor,
                QueryCommand = queryCommandDescriptor
            };
        }
        
        public static CommandDescriptorComposition CreateQueryResult(FullCommandDescriptor queryCommandDescriptor)
        {
            return new CommandDescriptorComposition
            {
                SessionCommand = null,
                QueryCommand = queryCommandDescriptor
            };
        }
    }
}