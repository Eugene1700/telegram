﻿using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IAuthProvider
    {
        Task<bool> AuthUser<TQuery>(long telegramUserId, ICommandExecutionContext<TQuery> context);
    }

    public abstract class ExecuteFilterAttribute
    {
        public abstract Task Filter<TQuery>(ICommandExecutionContext<TQuery> context);
    }
}