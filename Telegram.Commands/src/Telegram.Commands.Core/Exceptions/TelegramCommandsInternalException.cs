﻿using System;
using Telegram.Bot.Types;

namespace Telegram.Commands.Core.Exceptions
{
    public class TelegramCommandsInternalException : Exception
    {
        public TelegramCommandsInternalException(string message) : base(message)
        {
        }
    }
}