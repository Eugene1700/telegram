﻿using System;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract
{
    public class CommandSession<TData> : ISessionInfo
    {
        public string CommandQuery { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public long TelegramChatId { get; set; }
        public long TelegramUserId { get; set; }
        public TData Data { get; set; }
    }
}