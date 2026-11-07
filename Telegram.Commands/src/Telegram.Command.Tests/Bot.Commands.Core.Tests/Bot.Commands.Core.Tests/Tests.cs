using Microsoft.Extensions.DependencyInjection;
using Moq;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Tests.Core;
using Bot.Commands.Core.Tests.Environments;
using Bot.Commands.Core.Tests.Mocks;

namespace Bot.Commands.Core.Tests;

public class Tests : TestsBase
{
#pragma warning disable CS0649
    [ScopedInjection] private QueryMessageMockCommand _queryMessageMockCommand;
    [ScopedInjection] private SessionMessageMockCommand _sessionMessageMockCommand;
    [ScopedInjection] private BehaviorMockCommand _behaviorMockCommand;
    [ScopedInjection] private ICommandFactory _commandFactory;
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
        var result = CommandExecutionResult.Break();

        var newMessage = _messageEnvironment.CreateMessageQueryContext();
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
        var result = CommandExecutionResult.Freeze();

        var newMessage = _messageEnvironment.CreateMessageQueryContext();
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
        var result = CommandExecutionResult.Ahead<SessionMessageMockCommand, Message, SessionObjectMock>(
            new SessionObjectMock
            {
                Data = "123"
            }, (uint?)15);

        var newMessage = _messageEnvironment.CreateMessageQueryContext();
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
        var result = CommandExecutionResult.Ahead<SessionMessageMockCommand, Message, SessionObjectMock>(
            new SessionObjectMock
            {
                Data = "123"
            }, 66434, 15);

        var newMessage = _messageEnvironment.CreateMessageQueryContext();
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
        var result = CommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/sessionmessagemock_session", new SessionObjectMock
        {
            Data = "myData"
        });

        var newMessageContext = _messageEnvironment.CreateMessageQueryContext();
        var sessionCommandDescriptor = new FullCommandDescriptor(_sessionMessageMockCommand.GetType());
        Assert.That(sessionCommandDescriptor.IsQueryCommand, Is.False);
        Assert.That(sessionCommandDescriptor.IsBehaviorCommand, Is.False);
        Assert.That(sessionCommandDescriptor.IsSessionCommand, Is.True);
        _sessionMessageMockCommand.SetResult(result, (m, so) =>
        {
            Assert.That(m.Query.Text, Is.EqualTo(newMessageContext.GetData()));
            Assert.That(m.GetChatId(), Is.EqualTo(newMessageContext.GetChatId()));
            Assert.That(so, Is.Not.Null);
            Assert.That(so.Data, Is.EqualTo("myData"));
        });

        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateSessionResult(sessionCommandDescriptor),
            newMessageContext);

        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
        Assert.That(res.Data, Is.Null);
        Assert.That(res.NextCommandDescriptor, Is.Null);
        Assert.That(res.SessionDurationInSec, Is.Null);
        Assert.That(res.WaitFromChatId, Is.Null);
    }

    [Test]
    public async Task BehaviorCommandExecution()
    {
        var result = CommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/behaviormock_command", new SessionObjectMock
        {
            Data = "myData"
        });
        
        var newMessage = _messageEnvironment.CreateMessageQueryContext();
        var behaviorFullCommandDescriptor = new FullCommandDescriptor(_behaviorMockCommand.GetType());
        Assert.That(behaviorFullCommandDescriptor.IsQueryCommand, Is.False);
        Assert.That(behaviorFullCommandDescriptor.IsBehaviorCommand, Is.True);
        Assert.That(behaviorFullCommandDescriptor.IsSessionCommand, Is.False);
        
        _behaviorMockCommand.SetDefaultExecuteResult(result, (q, so) =>
        {
            var query = q as QueryContext<Message>;
            Assert.That(query, Is.Not.Null);
            Assert.That(query.GetData(), Is.EqualTo(newMessage.GetData()));
            Assert.That(query.GetChatId(), Is.EqualTo(newMessage.GetChatId()));
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
        var result = CommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/behaviormock_command", new SessionObjectMock
        {
            Data = "myData"
        });
        
        var newMessage = _messageEnvironment.CreateMessageQueryContext();
        var behaviorFullCommandDescriptor = new FullCommandDescriptor(_behaviorMockCommand.GetType());
        var sessionFullCommandDescriptor = new FullCommandDescriptor(_sessionMessageMockCommand.GetType());

        _behaviorMockCommand.SetSessionCommandExecuteResult(result, (cc, q, so) => {
            var currentCommand = cc as SessionMessageMockCommand;
            Assert.That(currentCommand, Is.Not.Null);
            
            var query = q as QueryContext<Message>;
            Assert.That(query, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(query.GetData(), Is.EqualTo(newMessage.GetData()));
                Assert.That(query.GetChatId(), Is.EqualTo(newMessage.GetChatId()));
                Assert.That(so, Is.Not.Null);
            });
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
        var result = CommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        SetSessionInfo(now, 123456789, 987654321, "/behaviormock_command", new SessionObjectMock
        {
            Data = "myData"
        });
        
        var newMessage = _messageEnvironment.CreateMessageQueryContext();
        var behaviorFullCommandDescriptor = new FullCommandDescriptor(_behaviorMockCommand.GetType());
        var queryFullCommandDescriptor = new FullCommandDescriptor(_queryMessageMockCommand.GetType());

        _behaviorMockCommand.SetQueryCommandExecuteResult(result, (cc, q, so) =>
        {
            var currentCommand = cc as QueryMessageMockCommand;
            Assert.That(currentCommand, Is.Not.Null);
            
            var query = q as QueryContext<Message>;
            Assert.That(query, Is.Not.Null);
            Assert.That(query.GetData(), Is.EqualTo("My Text"));
            Assert.That(query.GetChatId(), Is.EqualTo(123456789));
            Assert.That(so, Is.Not.Null);
            Assert.That(so.Data, Is.EqualTo("myData"));
        });

        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateBehaviorResult(behaviorFullCommandDescriptor, queryFullCommandDescriptor),
            newMessage);
        Assert.Multiple(() =>
        {
            Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
            Assert.That(res.Data, Is.Null);
            Assert.That(res.NextCommandDescriptor, Is.Null);
            Assert.That(res.SessionDurationInSec, Is.Null);
            Assert.That(res.WaitFromChatId, Is.Null);
        });
    }

    private async Task<ICommandExecutionResult> ExecuteCommandWithMessage(CommandExecutionResult result,
        QueryContext<Message> newMessage)
    {
        var queryCommandDescriptor = new FullCommandDescriptor(_queryMessageMockCommand.GetType());
        Assert.That(queryCommandDescriptor.IsQueryCommand, Is.True);
        Assert.That(queryCommandDescriptor.IsBehaviorCommand, Is.False);
        Assert.That(queryCommandDescriptor.IsSessionCommand, Is.False);
        _queryMessageMockCommand.SetResult(result, m =>
        {
            Assert.That(m.GetData(), Is.EqualTo("My Text"));
            Assert.That(m.GetChatId(), Is.EqualTo(123456789));
        });
        var res = await _commandExecutionService.Execute(
            CommandDescriptorComposition.CreateQueryResult(queryCommandDescriptor),
            newMessage);
        return res;
    }
    
    private void SetSessionInfo<TSessionData>(DateTime now, long chatId, long userId, string commandQuery,
        TSessionData sessionData)
    {
        _sessionStoreMock.SetSessionInfoWithDataCallback((n, c, tu, cq, t) =>
        {
            Assert.That(n, Is.EqualTo(now));
            Assert.That(c, Is.EqualTo(chatId));
            Assert.That(tu, Is.EqualTo(userId));
            Assert.That(cq, Is.EqualTo(commandQuery));
            Assert.That(t, Is.EqualTo(typeof(TSessionData)));
            return new SessionInfoMock
            {
                CommandQuery = cq,
                Data = sessionData,
                ExpiredAt = n.AddHours(1),
                OpenedAt = n,
                ChatId = c,
                UserId = tu
            };
        });
    }

    protected override void RegisterServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddServices();
        serviceCollection.AddScoped<CommandExecutionService>();
    }
}