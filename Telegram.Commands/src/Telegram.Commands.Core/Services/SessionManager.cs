﻿using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Services
{
    internal sealed class SessionManager : ISessionManager
    {
        private readonly IClock _clock;
        private readonly ISessionsStore _sessionsStore;

        public SessionManager(IClock clock, ISessionsStore sessionsStore)
        {
            _clock = clock;
            _sessionsStore = sessionsStore;
        }

        public ISessionInfo GetCurrentSession(long chatId, long telegramUserId)
        {
            var now = _clock.Now;
            return _sessionsStore.GetSessionInfo(now, chatId, telegramUserId);
        }
        
        public ISessionInfoWithData GetCurrentSession(long chatId, long telegramUserId, string commandQuery, Type sessionObjectType)
        {
            var now = _clock.Now;
            return _sessionsStore.GetSessionInfoWithData(now, chatId, telegramUserId, commandQuery, sessionObjectType);
        }

        private bool SessionIsNotExpired(ISessionInfo s)
        {
            var now = _clock.Now;
            return !s.ExpiredAt.HasValue || s.ExpiredAt > now;
        }

        public async Task<CommandSession> OpenSession(ITelegramCommandDescriptor nextCommandDescriptor, long chatId,
            long telegramUserId, object sessionData,
            uint? sessionTimeInSec = 600)
        {
            var now = _clock.Now;
            if (GetCurrentSession(chatId, telegramUserId) != null)
                throw new TelegramCommandsInternalException("Session has been already opened");
            var commandSession = new CommandSession
            {
                CommandQuery = nextCommandDescriptor.GetCommandQuery(),
                OpenedAt = now,
                TelegramChatId = chatId,
                TelegramUserId = telegramUserId,
                ExpiredAt = sessionTimeInSec.HasValue ? 
                    now.AddSeconds(sessionTimeInSec.Value) : (DateTime?) null,
                Data = sessionData
            };
            await _sessionsStore.CreateSession(commandSession);
            return commandSession;
        }

        public async Task<ISessionInfoWithData> ContinueSession(
            ITelegramCommandDescriptor nextCommandDescriptor, long chatIdFrom, long chatIdTo,
            long telegramUserId, object sessionData,
            uint? sessionTimeInSec = 600)
        {
            var session = GetCurrentSession(chatIdFrom, telegramUserId);
            if (!SessionIsNotExpired(session) || session == null)
                throw new TelegramCommandsInternalException("Session has been released");

            if (chatIdFrom != chatIdTo)
            {
                var sessionToChatSession
                    = GetCurrentSession(chatIdTo, telegramUserId);
                if (sessionToChatSession != null &&
                    sessionToChatSession.CommandQuery == nextCommandDescriptor.GetCommandQuery())
                    throw new TelegramCommandsInternalException("You must complete session in next chat");
            }

            var expiredTime = session.ExpiredAt;
            if (sessionTimeInSec.HasValue)
            {
                expiredTime = expiredTime?.AddSeconds(sessionTimeInSec.Value) 
                              ?? _clock.Now.AddSeconds(sessionTimeInSec.Value);
            }
            else
            {
                expiredTime = null;
            }
            var ses = CreateCommandSession(session, expiredTime,
                nextCommandDescriptor.GetCommandQuery(), chatIdTo, sessionData);
            
            await _sessionsStore.UpdateSession(ses, chatIdFrom);
            return ses;
        }

        public ISessionInfoWithData<TData> GetSession<TCommand, TQuery, TData>(long chatId, long telegramUserId)
            where TCommand : ISessionTelegramCommand<TQuery, TData>
        {
            var now = _clock.Now;
            var commandInfo = TelegramCommandExtensions.GetCommandInfo<TCommand, TQuery, TData>();
            var commandQuery = commandInfo.GetCommandQuery();
            var session = _sessionsStore.GetSessionInfoWithData<TData>(now, chatId, telegramUserId, commandQuery);
            if (session == null)
                return null;
            return commandQuery != session.CommandQuery ? null : session;
        }

        public ISessionInfoWithData<TData> GetSession<TCommand, TData>(long chatId, long telegramUserId) where TCommand : IBehaviorTelegramCommand<TData>
        {
            var now = _clock.Now;
            var commandInfo = TelegramCommandExtensions.GetBehaviorCommandInfo<TCommand, TData>();
            var commandQuery = commandInfo.GetCommandQuery();
            var session = _sessionsStore.GetSessionInfoWithData<TData>(now, chatId, telegramUserId, commandQuery);
            if (session == null)
                return null;
            return commandQuery != session.CommandQuery ? null : session;
        }

        private async Task ReleaseSessionInternal(ISessionInfo currentSession)
        {
            var commandSession = CreateCommandSession(currentSession, _clock.Now, currentSession.CommandQuery,
                currentSession.TelegramChatId, null);
            await _sessionsStore.UpdateSession(commandSession, currentSession.TelegramChatId);
        }

        private static CommandSession CreateCommandSession(ISessionInfo currentSession, DateTime? expiredAt,
            string commandQuery, long chatId, object sessionData)
        {
            return new CommandSession
            {
                CommandQuery = commandQuery,
                OpenedAt = currentSession.OpenedAt,
                TelegramChatId = chatId,
                TelegramUserId = currentSession.TelegramUserId,
                ExpiredAt = expiredAt,
                Data = sessionData
            };
        }

        public async Task ReleaseSessionIfExists(long chatId, long telegramUserId)
        {
            var currentSession = GetCurrentSession(chatId, telegramUserId);
            if (currentSession != null)
            {
                await ReleaseSessionInternal(currentSession);
            }
        }

        public async Task<ISessionInfoWithData> OpenSession<TCommand, TQuery, TData>(long chatId, long telegramUserId, TData sessionData, uint? sessionTimeInSec) where TCommand : ISessionTelegramCommand<TQuery, TData>
        {
            return await OpenSession(TelegramCommandExtensions.GetCommandInfo<TCommand, TQuery, TData>(), chatId, telegramUserId,
                sessionData, sessionTimeInSec);
        }

        public async Task<ISessionInfoWithData> OpenBehaviorSession<TCommand, TData>(long chatId, long telegramUserId, TData sessionData, uint? sessionTimeInSec) where TCommand : IBehaviorTelegramCommand<TData>
        {
            return await OpenSession(TelegramCommandExtensions.GetBehaviorCommandInfo<TCommand, TData>(), chatId, telegramUserId,
                sessionData, sessionTimeInSec);
        }
    }
    
    public static class SessionManagerExtensions 
    {
        public static Task<ISessionInfoWithData> OpenFluentSession<TCommand, TData, TStates>(this ISessionManager sessionManager, 
            long chatId,
            long telegramUserId,
            TData sessionData,
            uint? sessionTimeInSec = 600) where TCommand : FluentCommand<TData, TStates>
        {
            return sessionManager.OpenBehaviorSession<TCommand, FluentObject<TData, TStates>>(chatId, telegramUserId,
                new FluentObject<TData, TStates>(sessionData, FireType.HandleCurrent), sessionTimeInSec);
        }
        
        public static (ISessionInfo, TData) GetFluentSession<TCommand, TData, TStates>(this ISessionManager sessionManager, 
            long chatId,
            long telegramUserId) where TCommand : FluentCommand<TData, TStates>
        {
            var session = sessionManager.GetSession<TCommand, FluentObject<TData, TStates>>(chatId, telegramUserId);
            if (session == null)
            {
                return (null, default);
            }

            return (session, session.Data.Object);
        }
    }
}