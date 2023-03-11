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

public class Tests : TestsBase
{
    [ScopedInjection] private QueryMessageMockCommand _queryMessageMockCommand;
    [ScopedInjection] private SessionMessageMockCommand _sessionMessageMockCommand;
    [ScopedInjection] private ITelegramCommandFactory _telegramCommandFactory;
    [ScopedInjection] private CommandExecutionService _commandExecutionService;
    [ScopedInjection] private MessageEnvironment _messageEnvironment;
    [ScopedInjection] private SessionStoreMock _sessionStoreMock;
    [ScopedInjection] private ClockMock _clockMock;

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
    public async Task SessionCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Break();

        var now = new DateTime(2023, 03, 06);
        _clockMock.SetNow(now);

        _sessionStoreMock.SetSessionInfoWithDataCallback((n, c, tu, cq, t ) =>
        {
            Assert.That(n, Is.EqualTo(new DateTime(2023, 03, 06)));
            Assert.That(c, Is.EqualTo(123456789));
            Assert.That(tu, Is.EqualTo(987654321));
            Assert.That(cq, Is.EqualTo("/sessionmessagemock_session"));
            Assert.That(t, Is.EqualTo(typeof(SessionObjectMock)));
            return new SessionInfoMock
            {
                CommandQuery = cq,
                Data = new SessionObjectMock
                {
                    Data = "myData"
                },
                ExpiredAt = now.AddHours(1),
                OpenedAt = now,
                TelegramChatId = c,
                TelegramUserId = tu
            };
        });
        
        var newMessage = _messageEnvironment.CreateMessage();
        var sessionCommandDescriptor = new FullCommandDescriptor(_sessionMessageMockCommand.GetType());
        _sessionMessageMockCommand.SetResult(result, (m,so) =>
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

    private async Task<ITelegramCommandExecutionResult> ExecuteCommandWithMessage(TelegramCommandExecutionResult result, Message newMessage)
    {
        var queryCommandDescriptor = new FullCommandDescriptor(_queryMessageMockCommand.GetType());
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

    protected override void RegisterServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddServices();
        serviceCollection.AddScoped<CommandExecutionService>();
    }
}