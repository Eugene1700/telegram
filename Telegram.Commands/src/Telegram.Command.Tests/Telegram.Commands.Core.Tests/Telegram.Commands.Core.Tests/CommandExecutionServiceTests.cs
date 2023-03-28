using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;
using Telegram.Commands.Core.Tests.Core;
using Telegram.Commands.Core.Tests.Environments;
using Telegram.Commands.Core.Tests.Mocks;

namespace Telegram.Commands.Core.Tests;

public class CommandExecutionTests : TestsBase
{
#pragma warning disable CS0649
    [ScopedInjection] private QueryMessageMockCommand _queryMessageMockCommand;
    [ScopedInjection] private SessionMessageMockCommand _sessionMessageMockCommand;
    [ScopedInjection] private BehaviorMockCommand _behaviorMockCommand;
    [ScopedInjection] private ITelegramCommandFactory _telegramCommandFactory;
    [ScopedInjection] private CommandExecutionService _commandExecutionService;
    [ScopedInjection] private MessageEnvironment _messageEnvironment;
    [ScopedInjection] private SessionStoreMock _sessionStoreMock;
    [ScopedInjection] private ClockMock _clockMock;
#pragma warning restore CS0649

    [SetUp]
    public void Setup()
    {
        base.SetUp();
    }

    [Test]
    public async Task BreakResultQueryCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Break();

        var newMessage = _messageEnvironment.CreateMessage();
        var res = await ExecuteCommandWithMessage(result, newMessage);

        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
        Assert.That(res.Data, Is.Null);
        Assert.That(res.NextCommandDescriptor, Is.Null);
        Assert.That(res.SessionDurationInSec, Is.Null);
        Assert.That(res.WaitFromChatId, Is.Null);
    }

    [Test]
    public async Task FreezeResultQueryCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Freeze();

        var newMessage = _messageEnvironment.CreateMessage();
        var res = await ExecuteCommandWithMessage(result, newMessage);

        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Freeze));
        Assert.That(res.Data, Is.Null);
        Assert.That(res.NextCommandDescriptor, Is.Null);
        Assert.That(res.SessionDurationInSec, Is.Null);
        Assert.That(res.WaitFromChatId, Is.Null);
    }

    [Test]
    public async Task AheadResultQueryCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Ahead<SessionMessageMockCommand, Message, SessionObjectMock>(
            new SessionObjectMock
            {
                Data = "123"
            }, (uint?)15);

        var newMessage = _messageEnvironment.CreateMessage();
        var res = await ExecuteCommandWithMessage(result, newMessage);

        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Ahead));

        Assert.That(res.Data, Is.Not.Null);
        var resData = res.Data as SessionObjectMock;
        Assert.That(resData, Is.Not.Null);
        Assert.That(resData.Data, Is.EqualTo("123"));

        Assert.That(res.NextCommandDescriptor, Is.Not.Null);
        Assert.That(res.NextCommandDescriptor.Area, Is.EqualTo(ChatArea.Private));
        Assert.That(res.NextCommandDescriptor.Authorized, Is.True);
        Assert.That(res.NextCommandDescriptor.Swarms, Is.Null);

        Assert.That(res.SessionDurationInSec, Is.EqualTo(15));
        Assert.That(res.WaitFromChatId, Is.Null);
    }

    [Test]
    public async Task AheadToAnotherChatResultQueryCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Ahead<SessionMessageMockCommand, Message, SessionObjectMock>(
            new SessionObjectMock
            {
                Data = "123"
            }, 66434, 15);

        var newMessage = _messageEnvironment.CreateMessage();
        var res = await ExecuteCommandWithMessage(result, newMessage);

        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Ahead));

        Assert.That(res.Data, Is.Not.Null);
        var resData = res.Data as SessionObjectMock;
        Assert.That(resData, Is.Not.Null);
        Assert.That(resData.Data, Is.EqualTo("123"));

        Assert.That(res.NextCommandDescriptor, Is.Not.Null);
        Assert.That(res.NextCommandDescriptor.Area, Is.EqualTo(ChatArea.Private));
        Assert.That(res.NextCommandDescriptor.Authorized, Is.True);
        Assert.That(res.NextCommandDescriptor.Swarms, Is.Null);

        Assert.That(res.SessionDurationInSec, Is.EqualTo(15));
        Assert.That(res.WaitFromChatId, Is.EqualTo(66434));
    }

    [Test]
    public async Task SessionCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/sessionmessagemock_session", new SessionObjectMock
        {
            Data = "myData"
        });

        var newMessage = _messageEnvironment.CreateMessage();
        var sessionCommandDescriptor = new FullCommandDescriptor(_sessionMessageMockCommand.GetType());
        Assert.That(sessionCommandDescriptor.IsQueryCommand, Is.False);
        Assert.That(sessionCommandDescriptor.IsBehaviorTelegramCommand, Is.False);
        Assert.That(sessionCommandDescriptor.IsSessionTelegramCommand, Is.True);
        _sessionMessageMockCommand.SetResult(result, (m, so) =>
        {
            Assert.That(m.Text, Is.EqualTo(newMessage.Text));
            Assert.That(m.GetChatId(), Is.EqualTo(newMessage.Chat.Id));
            Assert.That(so, Is.Not.Null);
            Assert.That(so.Data, Is.EqualTo("myData"));
        });

        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateSessionResult(sessionCommandDescriptor),
            newMessage);

        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
        Assert.That(res.Data, Is.Null);
        Assert.That(res.NextCommandDescriptor, Is.Null);
        Assert.That(res.SessionDurationInSec, Is.Null);
        Assert.That(res.WaitFromChatId, Is.Null);
    }

    [Test]
    public async Task BehaviorCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/behaviormock_command", new SessionObjectMock
        {
            Data = "myData"
        });
        
        var newMessage = _messageEnvironment.CreateMessage();
        var behaviorFullCommandDescriptor = new FullCommandDescriptor(_behaviorMockCommand.GetType());
        Assert.That(behaviorFullCommandDescriptor.IsQueryCommand, Is.False);
        Assert.That(behaviorFullCommandDescriptor.IsBehaviorTelegramCommand, Is.True);
        Assert.That(behaviorFullCommandDescriptor.IsSessionTelegramCommand, Is.False);
        
        _behaviorMockCommand.SetDefaultExecuteResult(result, (q, so) =>
        {
            var query = q as Message;
            Assert.That(query, Is.Not.Null);
            Assert.That(query.Text, Is.EqualTo(newMessage.Text));
            Assert.That(query.GetChatId(), Is.EqualTo(newMessage.Chat.Id));
            Assert.That(so, Is.Not.Null);
            Assert.That(so.Data, Is.EqualTo("myData"));
        });

        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateBehaviorResult(behaviorFullCommandDescriptor),
            newMessage);
        
        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
        Assert.That(res.Data, Is.Null);
        Assert.That(res.NextCommandDescriptor, Is.Null);
        Assert.That(res.SessionDurationInSec, Is.Null);
        Assert.That(res.WaitFromChatId, Is.Null);
    }
    
    [Test]
    public async Task BehaviorCommandExecuteSessionCommand()
    {
        var result = TelegramCommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/behaviormock_command", new SessionObjectMock
        {
            Data = "myData"
        });
        
        var newMessage = _messageEnvironment.CreateMessage();
        var behaviorFullCommandDescriptor = new FullCommandDescriptor(_behaviorMockCommand.GetType());
        var sessionFullCommandDescriptor = new FullCommandDescriptor(_sessionMessageMockCommand.GetType());

        _behaviorMockCommand.SetSessionCommandExecuteResult(result, (cc, q, so) =>
        {
            var currentCommand = cc as SessionMessageMockCommand;
            Assert.That(currentCommand, Is.Not.Null);
            
            var query = q as Message;
            Assert.That(query, Is.Not.Null);
            Assert.That(query.Text, Is.EqualTo(newMessage.Text));
            Assert.That(query.GetChatId(), Is.EqualTo(newMessage.Chat.Id));
            Assert.That(so, Is.Not.Null);
            Assert.That(so.Data, Is.EqualTo("myData"));
        });

        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateBehaviorResult(behaviorFullCommandDescriptor, sessionFullCommandDescriptor),
            newMessage);
        
        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
        Assert.That(res.Data, Is.Null);
        Assert.That(res.NextCommandDescriptor, Is.Null);
        Assert.That(res.SessionDurationInSec, Is.Null);
        Assert.That(res.WaitFromChatId, Is.Null);
    }

    [Test]
    public async Task BehaviorCommandExecuteQueryCommand()
    {
        var result = TelegramCommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/behaviormock_command", new SessionObjectMock
        {
            Data = "myData"
        });
        
        var newMessage = _messageEnvironment.CreateMessage();
        var behaviorFullCommandDescriptor = new FullCommandDescriptor(_behaviorMockCommand.GetType());
        var queryFullCommandDescriptor = new FullCommandDescriptor(_queryMessageMockCommand.GetType());

        _behaviorMockCommand.SetQueryCommandExecuteResult(result, (cc, q, so) =>
        {
            var currentCommand = cc as QueryMessageMockCommand;
            Assert.That(currentCommand, Is.Not.Null);
            
            var query = q as Message;
            Assert.That(query, Is.Not.Null);
            Assert.That(query.Text, Is.EqualTo(newMessage.Text));
            Assert.That(query.GetChatId(), Is.EqualTo(newMessage.Chat.Id));
            Assert.That(so, Is.Not.Null);
            Assert.That(so.Data, Is.EqualTo("myData"));
        });

        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateBehaviorResult(behaviorFullCommandDescriptor, queryFullCommandDescriptor),
            newMessage);
        
        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
        Assert.That(res.Data, Is.Null);
        Assert.That(res.NextCommandDescriptor, Is.Null);
        Assert.That(res.SessionDurationInSec, Is.Null);
        Assert.That(res.WaitFromChatId, Is.Null);
    }
    
    private async Task<ITelegramCommandExecutionResult> ExecuteCommandWithMessage(TelegramCommandExecutionResult result,
        Message newMessage)
    {
        var queryCommandDescriptor = new FullCommandDescriptor(_queryMessageMockCommand.GetType());
        Assert.That(queryCommandDescriptor.IsQueryCommand, Is.True);
        Assert.That(queryCommandDescriptor.IsBehaviorTelegramCommand, Is.False);
        Assert.That(queryCommandDescriptor.IsSessionTelegramCommand, Is.False);
        _queryMessageMockCommand.SetResult(result, m =>
        {
            Assert.That(m.Text, Is.EqualTo(newMessage.Text));
            Assert.That(m.GetChatId(), Is.EqualTo(newMessage.Chat.Id));
        });
        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateQueryResult(queryCommandDescriptor),
            newMessage);
        return res;
    }
    
    private void SetSessionInfo<TSessionData>(DateTime now, long chatId, long telegramUserId, string commandQuery,
        TSessionData sessionData)
    {
        _sessionStoreMock.SetSessionInfoWithDataCallback((n, c, tu, cq, t) =>
        {
            Assert.That(n, Is.EqualTo(now));
            Assert.That(c, Is.EqualTo(chatId));
            Assert.That(tu, Is.EqualTo(telegramUserId));
            Assert.That(cq, Is.EqualTo(commandQuery));
            Assert.That(t, Is.EqualTo(typeof(TSessionData)));
            return new SessionInfoMock
            {
                CommandQuery = cq,
                Data = sessionData,
                ExpiredAt = n.AddHours(1),
                OpenedAt = n,
                TelegramChatId = c,
                TelegramUserId = tu
            };
        });
    }

    protected override void RegisterServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddServices();
        serviceCollection.AddScoped<CommandExecutionService>();
    }
}