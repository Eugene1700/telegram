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

    private static void AssertSession(ISessionInfo session, string commandQuery, DateTime? expiredAt, DateTime openedAt,
        long chatId, long telegramUserId)
    {
        Assert.That(session, Is.Not.Null);
        Assert.That(session.CommandQuery, Is.EqualTo(commandQuery));
        Assert.That(session.ExpiredAt, Is.EqualTo(expiredAt));
        Assert.That(session.OpenedAt, Is.EqualTo(openedAt));
        Assert.That(session.TelegramChatId, Is.EqualTo(chatId));
        Assert.That(session.TelegramUserId, Is.EqualTo(telegramUserId));
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

    [Test]
    public async Task GetCurrentSessionWithSessionObject()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);
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
        _sessionStoreMock.SetSessionInfoWithDataCallback(((n, chatId, telegramUserId, commandQuery, type) =>
        {
            Assert.That(n, Is.EqualTo(now));
            Assert.That(chatId, Is.EqualTo(sessionInfoMock.TelegramChatId));
            Assert.That(telegramUserId, Is.EqualTo(sessionInfoMock.TelegramUserId));
            Assert.That(commandQuery, Is.EqualTo(sessionInfoMock.CommandQuery));
            Assert.That(type, Is.EqualTo(typeof(SessionObjectMock)));
            return sessionInfoMock;
        }));
        var session = _sessionManager.GetCurrentSession(1234, 5678, "/myCommand", typeof(SessionObjectMock));
        AssertSession(session, "/myCommand", expiredAt, openedAt, 1234, 5678);
        var sessionObject = session.Data as SessionObjectMock;
        Assert.That(sessionObject, Is.Not.Null);
        Assert.That(sessionObject.Data, Is.EqualTo("myData"));
    }

    [Test]
    public async Task GetSession()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var (sessionInfoMock, _, openedAt, expiredAt) =
            _sessionEnvironment.CreateSessionInfoWithData("/sessionmessagemock_session");
        _sessionStoreMock.SetSessionInfoWithDataCallback<SessionObjectMock>(
            ((n, chatId, telegramUserId, commandQuery, type) =>
            {
                Assert.That(n, Is.EqualTo(now));
                Assert.That(chatId, Is.EqualTo(sessionInfoMock.TelegramChatId));
                Assert.That(telegramUserId, Is.EqualTo(sessionInfoMock.TelegramUserId));
                Assert.That(commandQuery, Is.EqualTo(sessionInfoMock.CommandQuery));
                Assert.That(type, Is.EqualTo(typeof(SessionObjectMock)));
                return sessionInfoMock;
            }));
        var session = _sessionManager.GetSession<SessionMessageMockCommand, Message, SessionObjectMock>(1234, 5678);
        AssertSession(session, "/sessionmessagemock_session", expiredAt, openedAt, 1234, 5678);
        var sessionObject = session.Data;
        Assert.That(sessionObject, Is.Not.Null);
        Assert.That(sessionObject.Data, Is.EqualTo("myData"));
    }

    [Test]
    public async Task GetSessionBehavior()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var (sessionInfoMock, _, openedAt, expiredAt) =
            _sessionEnvironment.CreateSessionInfoWithData("/behaviormock_command");
        _sessionStoreMock.SetSessionInfoWithDataCallback<SessionObjectMock>(
            ((n, chatId, telegramUserId, commandQuery, type) =>
            {
                Assert.That(n, Is.EqualTo(now));
                Assert.That(chatId, Is.EqualTo(sessionInfoMock.TelegramChatId));
                Assert.That(telegramUserId, Is.EqualTo(sessionInfoMock.TelegramUserId));
                Assert.That(commandQuery, Is.EqualTo(sessionInfoMock.CommandQuery));
                Assert.That(type, Is.EqualTo(typeof(SessionObjectMock)));
                return sessionInfoMock;
            }));
        var session = _sessionManager.GetSession<BehaviorMockCommand, SessionObjectMock>(1234, 5678);
        AssertSession(session, "/behaviormock_command", expiredAt, openedAt, 1234, 5678);
        var sessionObject = session.Data;
        Assert.That(sessionObject, Is.Not.Null);
        Assert.That(sessionObject.Data, Is.EqualTo("myData"));
    }

    [Test]
    public async Task GetSessionNull()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);

        _sessionStoreMock.SetSessionInfoWithData<SessionObjectMock>(null);
        var session = _sessionManager.GetSession<SessionMessageMockCommand, Message, SessionObjectMock>(1234, 5678);
        Assert.That(session, Is.Null);
    }

    [Test]
    public async Task GetSessionButCurrentAnother()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var (sessionInfo, _, _, _) = _sessionEnvironment.CreateSessionInfoWithData();

        _sessionStoreMock.SetSessionInfoWithData(sessionInfo);
        var session =
            _sessionManager.GetSession<SessionMessageMockCommand, Message, SessionObjectMock>(
                sessionInfo.TelegramChatId, sessionInfo.TelegramUserId);
        Assert.That(session, Is.Null);
    }

    [Test]
    public async Task ContinueSession()
    {
        var now = new DateTime(2023, 01, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var (sessionInfo, _,openedAt, expiredTime) = _sessionEnvironment.CreateSessionInfoWithData();

        var nextCommandDescriptor =
            TelegramCommandExtensions.GetBehaviorCommandInfo<BehaviorMockCommand, SessionObjectMock>();
        _sessionStoreMock.SetSessionInfo(sessionInfo);
        var session =
            await _sessionManager.ContinueSession(nextCommandDescriptor, sessionInfo.TelegramChatId,
                sessionInfo.TelegramChatId, sessionInfo.TelegramUserId, new SessionObjectMock
                {
                    Data = "newData"
                }, 500);
        AssertSession(session, "/behaviormock_command", expiredTime.Value.AddSeconds(500), openedAt, 1234, 5678);
        var data = session.Data as SessionObjectMock;
        Assert.That(data, Is.Not.Null);
        Assert.That(data.Data, Is.EqualTo("newData"));
    }
    
    [Test]
    public async Task ContinueSessionMoveToAnotherChat()
    {
        var now = new DateTime(2023, 01, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var (sessionInfo, _,openedAt, expiredTime) = _sessionEnvironment.CreateSessionInfoWithData();

        var nextCommandDescriptor =
            TelegramCommandExtensions.GetBehaviorCommandInfo<BehaviorMockCommand, SessionObjectMock>();
        _sessionStoreMock.SetSessionInfo(sessionInfo);
        var session =
            await _sessionManager.ContinueSession(nextCommandDescriptor, sessionInfo.TelegramChatId,
                8976, sessionInfo.TelegramUserId, new SessionObjectMock
                {
                    Data = "newData"
                });
        AssertSession(session, "/behaviormock_command", expiredTime.Value.AddSeconds(600), openedAt, 8976, 5678);
        var data = session.Data as SessionObjectMock;
        Assert.That(data, Is.Not.Null);
        Assert.That(data.Data, Is.EqualTo("newData"));
    }
    
    [Test]
    public async Task ContinueInfiniteSession()
    {
        var now = new DateTime(2023, 01, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var sessionInfo = _sessionEnvironment.CreateSessionInfo(null, now.AddSeconds(-1), null, "/myCommand");

        var nextCommandDescriptor =
            TelegramCommandExtensions.GetBehaviorCommandInfo<BehaviorMockCommand, SessionObjectMock>();
        _sessionStoreMock.SetSessionInfo(sessionInfo);
        var session =
            await _sessionManager.ContinueSession(nextCommandDescriptor, sessionInfo.TelegramChatId,
                sessionInfo.TelegramChatId, sessionInfo.TelegramUserId, new SessionObjectMock
                {
                    Data = "newData"
                }, 500);
        AssertSession(session, "/behaviormock_command", now.AddSeconds(500), now.AddSeconds(-1), 1234, 5678);
    }
    
    [Test]
    public async Task ContinueReleasedSession()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var (sessionInfo,_,_,_) = _sessionEnvironment.CreateSessionInfo();

        var nextCommandDescriptor =
            TelegramCommandExtensions.GetBehaviorCommandInfo<BehaviorMockCommand, SessionObjectMock>();
        _sessionStoreMock.SetSessionInfo(sessionInfo);
        var exception =
            Assert.ThrowsAsync<TelegramCommandsInternalException>(() => _sessionManager.ContinueSession(nextCommandDescriptor, sessionInfo.TelegramChatId,
                sessionInfo.TelegramChatId, sessionInfo.TelegramUserId, new SessionObjectMock
                {
                    Data = "newData"
                }, 500));
        Assert.That(exception.Message, Is.EqualTo("Session has been released"));
    }
    
    [Test]
    public async Task ContinueCurrentNullSession()
    {
        var now = new DateTime(2023, 03, 21, 0, 0, 0);
        _clockMock.SetNow(now);
        var nextCommandDescriptor =
            TelegramCommandExtensions.GetBehaviorCommandInfo<BehaviorMockCommand, SessionObjectMock>();
        _sessionStoreMock.SetSessionInfo(null);
        var exception =
            Assert.ThrowsAsync<TelegramCommandsInternalException>(() => _sessionManager.ContinueSession(nextCommandDescriptor, 1234,
                1234, 5678, new SessionObjectMock
                {
                    Data = "newData"
                }, 500));
        Assert.That(exception.Message, Is.EqualTo("Session has been released"));
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