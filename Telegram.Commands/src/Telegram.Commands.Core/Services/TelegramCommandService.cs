using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Services
{
    internal sealed class TelegramCommandService : ITelegramCommandResolver
    {
        private readonly ITelegramBotClient _telegramClient;
        private readonly IAuthProvider _authProvider;
        private readonly SessionManager _sessionManager;
        private readonly ITelegramBotProfile _telegramBotProfile;
        private readonly CommandExecutionService _commandExecutionService;

        public TelegramCommandService(ITelegramBotClient telegramClient,
            IAuthProvider authProvider,
            SessionManager sessionManager,
            ITelegramBotProfile telegramBotProfile,
            CommandExecutionService commandExecutionService)
        {
            _telegramClient = telegramClient;
            _authProvider = authProvider;
            _sessionManager = sessionManager;
            _telegramBotProfile = telegramBotProfile;
            _commandExecutionService = commandExecutionService;
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

        private CommandDescriptorComposition GetEvent<T>(T query)
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

            return CommandDescriptorComposition.CreateQueryResult(new FullCommandDescriptor(type));
        }

        private async Task QueryHandler<T>(T query)
        {
            try
            {
                var commandDesc = GetCommand(query);
                if (commandDesc == null)
                    return;

                await AssertAuth(query, commandDesc);
                AssertChatType(query, commandDesc);
                var commandExecutionResult = await _commandExecutionService.Execute(commandDesc, query);
                await Next(query, commandExecutionResult);
            }
            catch (TelegramExtractionCommandInternalException ex)
            {
                throw new TelegramExtractionCommandException(ex.Message, query.GetChatId());
            }
            catch (TelegramDomainException ex)
            {
                var message = query.AsMessage();
                await _telegramClient.SendTextMessageAsync(message.Chat.Id, ex.Message,
                    replyToMessageId: message.MessageId);
            }
        }

        private async Task Next<T>(T query, ITelegramCommandExecutionResult commandExecutionResult)
        {
            var chatId = query.GetChatId();
            var userId = query.GetFromId();
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

        private async Task AssertAuth<T>(T query, CommandDescriptorComposition commandDesc)
        {
            var chatId = query.GetChatId();
            if (commandDesc.SessionCommand?.Authorized ?? false)
            {
                if (!await _authProvider.AuthUser(query.GetFromId(), new CommandExecutionContext<T>(query, commandDesc.SessionCommand)))
                    throw new TelegramCommandsPermissionException("Unauthorized user", chatId);
            }

            if (commandDesc.QueryCommand?.Authorized ?? false)
            {
                if (!await _authProvider.AuthUser(query.GetFromId(), new CommandExecutionContext<T>(query, commandDesc.QueryCommand)))
                    throw new TelegramCommandsPermissionException("Unauthorized user", chatId);
            }
        }

        private CommandDescriptorComposition GetCommand<T>(T query)
        {
            var chatId = query.GetChatId();
            if (TryGetSessionCommandStr(query, out var commandStr))
            {
                var comDesc = FindCommand(commandStr, chatId);
                if (!comDesc.IsBehaviorTelegramCommand)
                    return CommandDescriptorComposition.CreateSessionResult(comDesc);
                if (!TryGetQueryCommandStr(query, out var currentCommandStr))
                    return CommandDescriptorComposition.CreateBehaviorResult(comDesc);
                var queryCommandDesc = FindCommand(currentCommandStr, chatId);
                return CommandDescriptorComposition.CreateBehaviorResult(comDesc, queryCommandDesc);
            }

            if (IsEvent(query))
            {
                return GetEvent(query);
            }

            if (!TryGetQueryCommandStr(query, out commandStr))
                if (query.IsGroupMessage())
                    return null;
                else
                    throw new TelegramExtractionCommandException("Could not extract command", chatId);

            var queryComDesc = FindCommand(commandStr, chatId);
            return CommandDescriptorComposition.CreateQueryResult(queryComDesc);
        }

        private FullCommandDescriptor FindCommand(string commandStr, long chatId)
        {
            var commandTypes = FindCommandByName(commandStr);
            var comDesc = ApplySwarm(chatId, commandTypes);
            if (comDesc == null)
            {
                throw new TelegramExtractionCommandException("Command without attribute", chatId);
            }

            return comDesc;
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

        private static void AssertChatType<T>(T query, CommandDescriptorComposition commandDescriptorComposition)
        {
            if (commandDescriptorComposition.IsBehaviorCommand)
            {
                AssertChatType(query, commandDescriptorComposition.SessionCommand.Descriptor);
                if (commandDescriptorComposition.QueryCommand != null) 
                    AssertChatType(query, commandDescriptorComposition.QueryCommand.Descriptor);
                return;
            }

            if (commandDescriptorComposition.IsSessionCommand)
            {
                AssertChatType(query, commandDescriptorComposition.SessionCommand.Descriptor);
                return;
            }

            AssertChatType(query, commandDescriptorComposition.QueryCommand.Descriptor);
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

        private static FullCommandDescriptor[] FindCommandByName(string queryString)
        {
            //todo need cache
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p =>
                {
                    var attrLoc = p.GetCustomAttribute<CommandAttribute>();
                    return p.IsClass && !p.IsAbstract && attrLoc != null && attrLoc.MatchCommand(queryString);
                }).Select(x => new FullCommandDescriptor(x)).ToArray();

            return commandTypes;
        }

        private FullCommandDescriptor ApplySwarm(long chatId, FullCommandDescriptor[] suspectedCommands)
        {
            var swarms = _telegramBotProfile.Swarms;
            if (_telegramBotProfile.Swarms != null && (_telegramBotProfile?.Swarms.Any() ?? false))
            {
                var descs = suspectedCommands
                    .Where(x =>
                        x.Descriptor.Swarms == null || x.Descriptor.Swarms.Any(y => swarms.Contains(y))).ToArray();
                if (descs.Length == 1)
                    return descs[0];

                throw new TelegramExtractionCommandException("Too many commands for query", chatId);
            }

            if (suspectedCommands.Length == 1)
                return suspectedCommands[0];

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