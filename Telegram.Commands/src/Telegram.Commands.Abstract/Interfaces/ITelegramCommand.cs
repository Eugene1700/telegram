﻿using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommand<in TQuery>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query);
    }
}