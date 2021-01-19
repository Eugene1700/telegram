using System;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionInfo
    {
        public string CommandQuery { get; }
        public DateTime OpenedAt { get;}
        public DateTime ExpiredAt { get; }
    }
}