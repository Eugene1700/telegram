using System;
using System.ComponentModel.DataAnnotations.Schema;
using EntityStorage.Core;

namespace SimpleHandlers.Domain
{
    [Table("COMMAND_SESSION")]
    public class CommandSession : StandardEntity
    {
        [Column("PLATFORM")]
        public BotPlatform Platform { get; set; } = BotPlatform.Telegram;

        [Column("COMMAND_QUERY")]
        public string CommandQuery { get; set; }
        
        [Column("OPENED_AT")]
        public DateTime OpenedAt { get; set; }
        
        [Column("EXPIRED_AT")]
        public DateTime? ExpiredAt { get; set; }

        [Column("ID_TELEGRAM_CHAT")]
        public long ChatId { get; set; }

        [Column("ID_TELEGRAM_USER")] 
        public long UserId { get; set; }
        
        [Column("SESSION_DATA")]
        public string SessionData { get; set; }
    }

    public enum BotPlatform { Telegram = 0, Max = 1 }

}
