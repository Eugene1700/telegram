using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Services;

public abstract class CommandServiceBase
{
    private readonly CommandExecutionService _commandExecutionService;
    private readonly SessionManager _sessionManager;
    private readonly IAuthProvider _authProvider;
    private readonly ITelegramBotProfile _telegramBotProfile;

    protected CommandServiceBase(CommandExecutionService commandExecutionService, SessionManager sessionManager,
        IAuthProvider authProvider, ITelegramBotProfile telegramBotProfile)
    {
        _commandExecutionService = commandExecutionService;
        _sessionManager = sessionManager;
        _authProvider = authProvider;
        _telegramBotProfile = telegramBotProfile;
    }

    protected async Task QueryHandler<TQuery>(QueryContext<TQuery> query)
    {
        try
        {
            var commandDesc = GetCommand(query);
            if (commandDesc == null)
                return;

            await AssertAuth(query, commandDesc);
            AssertChatType(query, commandDesc);
            var commandExecutionResult =
                await _commandExecutionService.Execute(commandDesc, query);
            await Next(query, commandExecutionResult);
        }
        catch (TelegramExtractionCommandInternalException ex)
        {
            throw new TelegramExtractionCommandException(ex.Message, query.GetChatId());
        }
        catch (TelegramDomainException ex)
        {
            await SendDomainException(query, ex);
        }
    }

    protected abstract Task SendDomainException<TQuery>(QueryContext<TQuery> query, TelegramDomainException ex);

    private async Task Next<TQuery>(QueryContext<TQuery> query, ITelegramCommandExecutionResult commandExecutionResult)
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

    private async Task AssertAuth<TQuery>(QueryContext<TQuery> query, CommandDescriptorComposition commandDesc)
    {
        var chatId = query.GetChatId();
        var userId = query.GetFromId();
        if (commandDesc.SessionCommand?.Authorized ?? false)
        {
            if (!await _authProvider.AuthUser(userId,
                    new CommandExecutionContext<TQuery>(query.Query, commandDesc.SessionCommand, chatId)))
                throw new TelegramCommandsPermissionException("Unauthorized user", chatId);
        }

        if (commandDesc.QueryCommand?.Authorized ?? false)
        {
            if (!await _authProvider.AuthUser(userId,
                    new CommandExecutionContext<TQuery>(query.Query, commandDesc.QueryCommand, chatId)))
                throw new TelegramCommandsPermissionException("Unauthorized user", chatId);
        }
    }

    private CommandDescriptorComposition GetCommand<TQuery>(QueryContext<TQuery> query)
    {
        if (query.IsEvent())
        {
            return GetEvent(query);
        }
        
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

        if (!TryGetQueryCommandStr(query, out commandStr))
            if (query.IsGroupMessage())
                return null;
            else
                throw new TelegramExtractionCommandException("Could not extract command", chatId);

        var queryComDesc = FindCommand(commandStr, chatId);
        return CommandDescriptorComposition.CreateQueryResult(queryComDesc);
    }

    private static CommandDescriptorComposition GetEvent<TQuery>(QueryContext<TQuery> query)
    {
        if (!query.IsEvent())
            return null;

        var eventType = query.GetEventType();
        if (eventType == null)
        {
            return null;
        }
        var type = FindEventByType(eventType.Value);
        return CommandDescriptorComposition.CreateQueryResult(new FullCommandDescriptor(type));
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

    private void AssertChatType<TQuery>(QueryContext<TQuery> query, CommandDescriptorComposition commandDescriptorComposition)
    {
        if (commandDescriptorComposition.IsBehaviorCommand)
        {
            if (commandDescriptorComposition.SessionCommand != null)
                AssertChatTypeConcrete(query, commandDescriptorComposition.SessionCommand.Descriptor);
            if (commandDescriptorComposition.QueryCommand != null)
                AssertChatTypeConcrete(query, commandDescriptorComposition.QueryCommand.Descriptor);
            return;
        }

        if (commandDescriptorComposition.IsSessionCommand)
        {
            AssertChatTypeConcrete(query, commandDescriptorComposition.SessionCommand.Descriptor);
            return;
        }

        AssertChatTypeConcrete(query, commandDescriptorComposition.QueryCommand.Descriptor);
    }
    
    private void AssertChatTypeConcrete<TQuery>(QueryContext<TQuery> query, ITelegramCommandDescriptor commandInfo)
    {
        if (commandInfo == null)
            throw new ArgumentNullException(nameof(commandInfo));
        var chatType = query.GetChatType();
        var chatId = query.GetChatId();
        switch (chatType)
        {
            case ChatArea.Private:
                if ((commandInfo.Area & ChatArea.Private) != ChatArea.Private)
                    throw new TelegramCommandsChatAreaException("This command is not for private chat", chatId);
                return;
            case ChatArea.Group:
                if ((commandInfo.Area & ChatArea.Group) != ChatArea.Group)
                    throw new TelegramCommandsChatAreaException("This command is not for group", chatId);
                return;
            case ChatArea.Channel:
                if ((commandInfo.Area & ChatArea.Channel) != ChatArea.Channel)
                    throw new TelegramCommandsChatAreaException("This command is not for channel", chatId);
                return;
            case ChatArea.SuperGroup:
                if ((commandInfo.Area & ChatArea.SuperGroup) != ChatArea.SuperGroup)
                    throw new TelegramCommandsChatAreaException("This command is not for supergroup", chatId);
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool TryGetSessionCommandStr<TQuery>(QueryContext<TQuery> query, out string commandStr)
    {
        commandStr = null;
        var chatId = query.GetChatId();
        var userId = query.GetFromId();
        var sessionInfo = _sessionManager.GetCurrentSession(chatId, userId);
        if (sessionInfo == null) return false;
        commandStr = sessionInfo.CommandQuery;
        return true;
    }

    private bool TryGetQueryCommandStr<TQuery>(QueryContext<TQuery> query, out string commandStr)
    {
        //todo pameters case
        commandStr = query.GetData();
        var isCommand = !string.IsNullOrWhiteSpace(commandStr) && commandStr[0] == '/';
        if (!isCommand)
            return false;
        if (!query.IsGroupMessage()) return true;
        var botName = _telegramBotProfile.BotName;
        return commandStr.EndsWith($"@{botName}"); //todo 
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

    protected static Type FindEventByType(EventType eventType)
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