using Telegram.Commands.Core.Tests.Mocks;

namespace Telegram.Commands.Core.Tests.Environments;

public class SessionEnvironment
{
    public SessionObjectMock CreateObject(string data = "myData")
    {
        return new SessionObjectMock
        {
            Data = data
        };
    }
    
    public SessionInfoMock CreateSessionInfo(SessionObjectMock sessionObjectMock, DateTime openedAt, DateTime? expiredAt)
    {
        return new SessionInfoMock
        {
            Data = sessionObjectMock,
            CommandQuery = "/myCommand",
            ExpiredAt = expiredAt,
            OpenedAt = openedAt,
            TelegramChatId = 1234,
            TelegramUserId = 5678
        };
    }
    
    public (SessionInfoMock,SessionObjectMock,DateTime, DateTime?) CreateSessionInfo()
    {
        var sessionObjectMock = CreateObject();
        var expiredAt = new DateTime(2023, 02, 01, 01, 02, 03);
        var openedAt = new DateTime(2022, 02, 01, 01, 02, 03);
        return (CreateSessionInfo(sessionObjectMock, openedAt, expiredAt), sessionObjectMock, openedAt, expiredAt);
    }
}