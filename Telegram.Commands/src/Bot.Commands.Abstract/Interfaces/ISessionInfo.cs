using System;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface ISessionInfo
    {
        public string CommandQuery { get; }
        public DateTime OpenedAt { get;}
        public DateTime? ExpiredAt { get; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
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