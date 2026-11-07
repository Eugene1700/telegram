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
}