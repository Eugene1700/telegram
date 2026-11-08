using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Services;

public abstract class CommandServiceBase
{
    private readonly CommandExecutionService _commandExecutionService;
    private readonly SessionManager _sessionManager;
    private readonly IAuthProvider _authProvider;
    private readonly IBotProfile _botProfile;
    protected IBotProfile BotProfile => _botProfile;

    protected CommandServiceBase(CommandExecutionService commandExecutionService, SessionManager sessionManager,
        IAuthProvider authProvider, IBotProfile botProfile)
    {
        _commandExecutionService = commandExecutionService;
        _sessionManager = sessionManager;
        _authProvider = authProvider;
        _botProfile = botProfile;
    }

    protected Task QueryHandler<TQuery>(IQueryContext<TQuery> query, IEnumerable<ICommandMatcher<TQuery>> matchers = null)
        => QueryHandler(query, matchers, 0);

    private async Task QueryHandler<TQuery>(IQueryContext<TQuery> query, IEnumerable<ICommandMatcher<TQuery>> matchers,
        int immediateTransitions)
    {
        try
        {
            var commandDesc = GetCommand(query, matchers);
            if (commandDesc == null)
                return;

            await AssertAuth(query, commandDesc);
            AssertChatType(query, commandDesc);
            await ApplyExecuteFilters(query, commandDesc);
            var commandExecutionResult =
                await _commandExecutionService.Execute(commandDesc, query);
            await Next(query, commandExecutionResult, immediateTransitions);
        }
        catch (CommandExtractionInternalException ex)
        {
            throw new CommandExtractionException(ex.Message, query.GetChatId());
        }
    }

    private static async Task ApplyExecuteFilters<TQuery>(IQueryContext<TQuery> query,
        CommandDescriptorComposition commandDesc)
    {
        var commands = new[] { commandDesc.SessionCommand, commandDesc.QueryCommand }
            .Where(command => command != null)
            .GroupBy(command => command.Type)
            .Select(group => group.First());

        foreach (var command in commands)
        {
            var context = new CommandExecutionContext<TQuery>(query.Query, command, query.GetChatId());
            foreach (var filter in command.Type.GetCustomAttributes<ExecuteFilterAttribute>(inherit: true)
                         .OrderBy(filter => filter.Order))
                await filter.Filter(context);
        }
    }

    private async Task Next<TQuery>(IQueryContext<TQuery> query, ICommandExecutionResult commandExecutionResult, int immediateTransitions)
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

        if (commandExecutionResult.Result == ExecuteResult.Ahead || commandExecutionResult.Result == ExecuteResult.Fire)
        {
            var fire = commandExecutionResult.Result == ExecuteResult.Fire;
            if (fire && ++immediateTransitions > 64)
                throw new InvalidOperationException("Too many immediate command transitions in one update.");
            if (fire && commandExecutionResult.WaitFromChatId != null)
                throw new InvalidOperationException("Immediate execution cannot move to another chat.");
            var data = commandExecutionResult.Data;
            if (data is Fluent.Routing.FluentRouteResume resume)
            {
                var destination = FindCommand<TQuery>(commandExecutionResult.NextCommandDescriptor.GetCommandQuery(), chatId);
                data = resume.Restore(destination.GetSessionObjectType(typeof(TQuery)));
            }
            var activeSession = _sessionManager.GetCurrentSession(chatId, userId);
            if (activeSession == null)
                await _sessionManager.OpenSession(commandExecutionResult.NextCommandDescriptor,
                    sessionChatId, userId, data, commandExecutionResult.SessionDurationInSec);
            else
                await _sessionManager.ContinueSession(commandExecutionResult.NextCommandDescriptor,
                    chatId, sessionChatId, userId, data, commandExecutionResult.SessionDurationInSec);
            if (fire) await QueryHandler(query, null, immediateTransitions);
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(commandExecutionResult.Result),
            commandExecutionResult.Result.ToString());
    }

    private async Task AssertAuth<TQuery>(IQueryContext<TQuery> query, CommandDescriptorComposition commandDesc)
    {
        var chatId = query.GetChatId();
        var userId = query.GetFromId();
        if (commandDesc.SessionCommand?.Authorized ?? false)
        {
            if (!await _authProvider.AuthUser(userId,
                    new CommandExecutionContext<TQuery>(query.Query, commandDesc.SessionCommand, chatId)))
                throw new CommandPermissionException("Unauthorized user", chatId);
        }

        if (commandDesc.QueryCommand?.Authorized ?? false)
        {
            if (!await _authProvider.AuthUser(userId,
                    new CommandExecutionContext<TQuery>(query.Query, commandDesc.QueryCommand, chatId)))
                throw new CommandPermissionException("Unauthorized user", chatId);
        }
    }

    private CommandDescriptorComposition GetCommand<TQuery>(IQueryContext<TQuery> query, IEnumerable<ICommandMatcher<TQuery>> matchers)
    {
        if (query.IsEvent())
        {
            return GetEvent(query);
        }
        
        var chatId = query.GetChatId();
        if (TryGetSessionCommandStr(query, out var commandStr))
        {
            var comDesc = FindCommand<TQuery>(commandStr, chatId);
            if (!comDesc.IsBehaviorCommand)
                return CommandDescriptorComposition.CreateSessionResult(comDesc);
            if (!TryGetQueryCommandStr(query, out var currentCommandStr))
                return CommandDescriptorComposition.CreateBehaviorResult(comDesc);
            var queryCommandDesc = FindCommand<TQuery>(currentCommandStr, chatId);
            if (queryCommandDesc.IsBehaviorCommand)
                return CommandDescriptorComposition.CreateBehaviorResult(comDesc);
            return CommandDescriptorComposition.CreateBehaviorResult(comDesc, queryCommandDesc);
        }

        if (!TryGetQueryCommandStr(query, out commandStr))
        {
            // A rejected slash command (for example, addressed to another bot) must not start a funnel.
            if (query.GetData()?.StartsWith("/", StringComparison.Ordinal) != true)
            {
                var matched = FindMatchedCommand(query, matchers);
                if (matched != null) return CommandDescriptorComposition.CreateQueryResult(matched);
            }
            if (query.IsGroupMessage())
                return null;
            else
                throw new CommandExtractionException("Could not extract command", chatId);
        }

        var queryComDesc = FindCommand<TQuery>(commandStr, chatId);
        return CommandDescriptorComposition.CreateQueryResult(queryComDesc);
    }

    private FullCommandDescriptor FindMatchedCommand<TQuery>(IQueryContext<TQuery> query, IEnumerable<ICommandMatcher<TQuery>> matchers)
    {
        var matched = new List<FullCommandDescriptor>();
        foreach (var matcher in matchers ?? Array.Empty<ICommandMatcher<TQuery>>())
        {
            var descriptor = BotCommandExtensions.GetCommandInfo(matcher.CommandType);
            var candidates = FindCommandByName("/" + descriptor.Name)
                .Where(d => d.SupportsQuery(typeof(TQuery)) && (d.IsQueryCommand || d.IsBehaviorCommand))
                .Where(d => _botProfile.Swarms == null || _botProfile.Swarms.Length == 0 ||
                    d.Descriptor.Swarms == null || d.Descriptor.Swarms.Any(s => _botProfile.Swarms.Contains(s)))
                .ToArray();
            if (candidates.Length > 0 && matcher.IsMatch(query)) matched.AddRange(candidates);
        }
        var distinct = matched.GroupBy(d => d.Type).Select(g => g.First()).ToArray();
        if (distinct.Length > 1)
            throw new CommandExtractionException("Multiple commands matched this update", query.GetChatId());
        return distinct.SingleOrDefault();
    }

    private static CommandDescriptorComposition GetEvent<TQuery>(IQueryContext<TQuery> query)
    {
        if (!query.IsEvent())
            return null;

        var eventType = query.GetEventType();
        if (eventType == null)
        {
            return null;
        }
        var descriptor = CommandTypeDiscovery.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters &&
                t.GetCustomAttribute<EventAttribute>()?.Type == eventType.Value)
            .Select(t => new FullCommandDescriptor(t))
            .SingleOrDefault(d => d.SupportsQuery(typeof(TQuery)));
        return descriptor == null ? null : CommandDescriptorComposition.CreateQueryResult(descriptor);
    }

    private FullCommandDescriptor FindCommand<TQuery>(string commandStr, long chatId)
    {
        var commandTypes = FindCommandByName(commandStr).Where(d => d.SupportsQuery(typeof(TQuery))).ToArray();
        var comDesc = ApplySwarm(chatId, commandTypes);
        if (comDesc == null)
        {
            throw new CommandExtractionException("Command without attribute", chatId);
        }

        return comDesc;
    }

    private void AssertChatType<TQuery>(IQueryContext<TQuery> query, CommandDescriptorComposition commandDescriptorComposition)
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
    
    private void AssertChatTypeConcrete<TQuery>(IQueryContext<TQuery> query, ICommandDescriptor commandInfo)
    {
        if (commandInfo == null)
            throw new ArgumentNullException(nameof(commandInfo));
        var chatType = query.GetChatType();
        var chatId = query.GetChatId();
        switch (chatType)
        {
            case ChatArea.Private:
                if ((commandInfo.Area & ChatArea.Private) != ChatArea.Private)
                    throw new CommandChatAreaException("This command is not for private chat", chatId);
                return;
            case ChatArea.Group:
                if ((commandInfo.Area & ChatArea.Group) != ChatArea.Group)
                    throw new CommandChatAreaException("This command is not for group", chatId);
                return;
            case ChatArea.Channel:
                if ((commandInfo.Area & ChatArea.Channel) != ChatArea.Channel)
                    throw new CommandChatAreaException("This command is not for channel", chatId);
                return;
            case ChatArea.SuperGroup:
                if ((commandInfo.Area & ChatArea.SuperGroup) != ChatArea.SuperGroup)
                    throw new CommandChatAreaException("This command is not for supergroup", chatId);
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool TryGetSessionCommandStr<TQuery>(IQueryContext<TQuery> query, out string commandStr)
    {
        commandStr = null;
        var chatId = query.GetChatId();
        var userId = query.GetFromId();
        var sessionInfo = _sessionManager.GetCurrentSession(chatId, userId);
        if (sessionInfo == null) return false;
        commandStr = sessionInfo.CommandQuery;
        return true;
    }

    protected virtual bool TryGetQueryCommandStr<TQuery>(IQueryContext<TQuery> query, out string commandStr)
    {
        commandStr = query.GetData();
        var isCommand = !string.IsNullOrWhiteSpace(commandStr) && commandStr[0] == '/';
        if (!isCommand)
            return false;
        return true;
    }
    private static FullCommandDescriptor[] FindCommandByName(string queryString)
    {
        //todo need cache
        var commandTypes = CommandTypeDiscovery.GetTypes()
            .Where(p =>
            {
                var attrLoc = p.GetCustomAttribute<CommandAttribute>();
                return p.IsClass && !p.IsAbstract && !p.ContainsGenericParameters &&
                    attrLoc != null && !(attrLoc is EventAttribute) && attrLoc.MatchCommand(queryString);
            }).Select(x => new FullCommandDescriptor(x)).ToArray();

        return commandTypes;
    }

    private FullCommandDescriptor ApplySwarm(long chatId, FullCommandDescriptor[] suspectedCommands)
    {
        var swarms = _botProfile.Swarms;
        if (_botProfile.Swarms != null && (_botProfile?.Swarms.Any() ?? false))
        {
            var descs = suspectedCommands
                .Where(x =>
                    x.Descriptor.Swarms == null || x.Descriptor.Swarms.Any(y => swarms.Contains(y))).ToArray();
            if (descs.Length == 1)
                return descs[0];

            throw new CommandExtractionException("Too many commands for query", chatId);
        }

        if (suspectedCommands.Length == 1)
            return suspectedCommands[0];

        throw new CommandExtractionException("Too many commands for query", chatId);
    }

    protected static Type FindEventByType(EventType eventType)
    {
        //todo need cache
        var commandType = CommandTypeDiscovery.GetTypes()
            .SingleOrDefault(p =>
            {
                var attrLoc = p.GetCustomAttribute<EventAttribute>();
                return p.IsClass && !p.IsAbstract && attrLoc != null && attrLoc.Type == eventType;
            });

        return commandType;
    }
}
