using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Services;
using Telegram.Commands.Core.Tests.Core;
using Telegram.Commands.Core.Tests.Environments;
using Telegram.Commands.Core.Tests.Mocks;

namespace Telegram.Commands.Core.Tests;

public class SessionMangerTests : TestsBase
{
    [ScopedInjection] private SessionManager _sessionManager;
    [ScopedInjection] private SessionMessageMockCommand _sessionMessageMockCommand;
    [ScopedInjection] private SessionStoreMock _sessionStoreMock;
    [ScopedInjection] private ClockMock _clockMock;
    [ScopedInjection] private SessionEnvironment _sessionEnvironment;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
    }

    [Test]
    public async Task OpenSession()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        var expiredAt = now.AddMinutes(10);
        _clockMock.SetNow(now);
        _sessionStoreMock.SetCreateSessionCallback(x =>
        {
            Assert.That(x.Data, Is.Null);
            AssertSession(x, "/sessionmessagemock_session", expiredAt, now, 1234, 5678);
        });

        var comDesc = _sessionMessageMockCommand.GetCommandInfo();
        var commandSession = await _sessionManager.OpenSession(comDesc, 1234, 5678, null);
        AssertSession(commandSession, "/sessionmessagemock_session", expiredAt, now, 1234, 5678);
        Assert.That(commandSession.Data, Is.Null);
    }

    [Test]
    public async Task OpenSessionWithDataAndExpiredExplicitly()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        var expiredAt = now.AddMinutes(5);
        _clockMock.SetNow(now);

        var sessionObjectMock = new SessionObjectMock
        {
            Data = "myData"
        };

        _sessionStoreMock.SetCreateSessionCallback(x =>
        {
            var typedData = AssertType<SessionObjectMock>(x.Data);
            Assert.That(typedData.Data, Is.EqualTo("myData"));
            AssertSession(x, "/sessionmessagemock_session", expiredAt, now, 1234, 5678);
        });

        var comDesc = _sessionMessageMockCommand.GetCommandInfo();
        var commandSession = await _sessionManager.OpenSession(comDesc, 1234, 5678, sessionObjectMock, 5 * 60);
        AssertSession(commandSession, "/sessionmessagemock_session", expiredAt, now, 1234, 5678);
        var typedData = AssertType<SessionObjectMock>(commandSession.Data);
        Assert.That(typedData.Data, Is.EqualTo("myData"));
    }

    [Test]
    public async Task OpenSessionInfiniteSession()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);

        _sessionStoreMock.SetCreateSessionCallback(x =>
        {
            AssertSession(x, "/sessionmessagemock_session", null, now, 1234, 5678);
            Assert.That(x.Data, Is.Null);
        });

        var comDesc = _sessionMessageMockCommand.GetCommandInfo();
        var commandSession = await _sessionManager.OpenSession(comDesc, 1234, 5678, null, null);
        AssertSession(commandSession, "/sessionmessagemock_session", null, now, 1234, 5678);
        Assert.That(commandSession.Data, Is.Null);
    }

    [Test]
    public async Task OpenSessionFromCommand()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);

        var sessionObjectMock = new SessionObjectMock
        {
            Data = "myData"
        };

        _sessionStoreMock.SetCreateSessionCallback(x =>
        {
            AssertSession(x, "/sessionmessagemock_session", null, now, 1234, 5678);
            var sessionObject = AssertType<SessionObjectMock>(x.Data);
            Assert.That(sessionObject.Data, Is.EqualTo("myData"));
        });

        var commandSession =
            await _sessionManager.OpenSession<SessionMessageMockCommand, Message, SessionObjectMock>(1234, 5678,
                sessionObjectMock,
                null);
        AssertSession(commandSession, "/sessionmessagemock_session", null, now, 1234, 5678);
        var sessionObject = AssertType<SessionObjectMock>(commandSession.Data);
        Assert.That(sessionObject.Data, Is.EqualTo("myData"));
    }

    private static void AssertSession(ISessionInfo x, string commandQuery, DateTime? expiredAt, DateTime now,
        long chatId, long telegramUserId)
    {
        Assert.That(x.CommandQuery, Is.EqualTo(commandQuery));
        Assert.That(x.ExpiredAt, Is.EqualTo(expiredAt));
        Assert.That(x.OpenedAt, Is.EqualTo(now));
        Assert.That(x.TelegramChatId, Is.EqualTo(chatId));
        Assert.That(x.TelegramUserId, Is.EqualTo(telegramUserId));
    }

    [Test]
    public async Task GetCurrentSession()
    {
        var sessionObjectMock = _sessionEnvironment.CreateObject();
        var expiredAt = new DateTime(2023, 02, 01, 01, 02, 03);
        var openedAt = new DateTime(2022, 02, 01, 01, 02, 03);
        var sessionInfoMock = new SessionInfoMock
        {
            Data = sessionObjectMock,
            CommandQuery = "/myCommand",
            ExpiredAt = expiredAt,
            OpenedAt = openedAt,
            TelegramChatId = 1234,
            TelegramUserId = 5678
        };
        _sessionStoreMock.SetSessionInfo(sessionInfoMock);
        var session = _sessionManager.GetCurrentSession(1234, 5678);
        AssertSession(session, "/myCommand", expiredAt, openedAt, 1234, 5678);
    }

    [Test]
    public async Task OpenSessionWhenAnotherHasBeenAlreadyOpened()
    {
        var (sessionInfoMock, _, _, _) = _sessionEnvironment.CreateSessionInfo();
        _sessionStoreMock.SetSessionInfo(sessionInfoMock);
        var ex = Assert.ThrowsAsync<TelegramCommandsInternalException>(() =>
                _sessionManager.OpenSession<SessionMessageMockCommand, Message, SessionObjectMock>(1234, 5678, null,
                    null));
        Assert.That(ex.Message, Is.EqualTo("Session has been already opened"));
    }

    protected override void RegisterServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddServices();
    }

    private static TType AssertType<TType>(object data) where TType : class
    {
        Assert.That(data, Is.Not.Null);
        var typedData = data as TType;
        Assert.That(typedData, Is.Not.Null);
        return typedData;
    }
}