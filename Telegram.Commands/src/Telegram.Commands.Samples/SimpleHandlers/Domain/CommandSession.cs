using System;
using System.ComponentModel.DataAnnotations.Schema;
using EntityStorage.Core;

namespace SimpleHandlers.Domain
{
    [Table("COMMAND_SESSION")]
    public class CommandSession : StandardEntity
    {
        [Column("COMMAND_QUERY")]
        public string CommandQuery { get; set; }
        
        [Column("OPENED_AT")]
        public DateTime OpenedAt { get; set; }
        
        [Column("EXPIRED_AT")]
        public DateTime? ExpiredAt { get; set; }

        [Column("ID_TELEGRAM_CHAT")]
        public long TelegramChatId { get; set; }

        [Column("ID_TELEGRAM_USER")] 
        public long TelegramUserId { get; set; }
        
        [Column("SESSION_DATA")]
        public string SessionData { get; set; }
    }

}