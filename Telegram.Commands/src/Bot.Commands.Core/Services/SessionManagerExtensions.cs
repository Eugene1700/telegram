using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Services
{

    public static class SessionManagerExtensions
    {
        public static Task<ISessionInfoWithData> OpenFluentSession<TCommand, TData, TStates>(this ISessionManager sessionManager,
            long chatId,
            long userId,
            TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : FluentCommand<TData, TStates>
        {
            return sessionManager.OpenBehaviorSession<TCommand, FluentObject<TData, TStates>>(chatId, userId,
                new FluentObject<TData, TStates>(sessionData, FireType.HandleCurrent), sessionTimeInSec);
        }

        public static (ISessionInfo, TData) GetFluentSession<TCommand, TData, TStates>(this ISessionManager sessionManager,
            long chatId,
            long userId) where TCommand : FluentCommand<TData, TStates>
        {
            var session = sessionManager.GetSession<TCommand, FluentObject<TData, TStates>>(chatId, userId);
            if (session == null)
            {
                return (null, default);
            }

            return (session, session.Data.Object);
        }
    }
}
