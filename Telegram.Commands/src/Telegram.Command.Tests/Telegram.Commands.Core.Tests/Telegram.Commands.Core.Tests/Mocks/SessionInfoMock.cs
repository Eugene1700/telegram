using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Tests.Mocks;

public class SessionInfoMock: ISessionInfoWithData
{
    public string CommandQuery { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public long TelegramChatId { get; set; }
    public long TelegramUserId { get; set; }
    public object Data { get; set; }
}