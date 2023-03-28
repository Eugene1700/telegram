using System;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionInfo
    {
        public string CommandQuery { get; }
        public DateTime OpenedAt { get;}
        public DateTime? ExpiredAt { get; }
        public long TelegramChatId { get; }
        public long TelegramUserId { get; }
    }

    public interface ISessionInfoWithData : ISessionInfo
    {
        public object Data { get; }
    }
    
    public interface ISessionInfoWithData<out T> : ISessionInfo
    {
        public T Data { get; }
    }
}