using System;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Models
{
    public class CommandSession : ISessionInfoWithData
    {
        public string CommandQuery { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public object Data { get; set; }
    }
}