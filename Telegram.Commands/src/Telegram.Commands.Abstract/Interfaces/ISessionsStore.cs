﻿using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionsStore
    {
        ISessionInfo GetSessionInfo(DateTime now, long chatId, long telegramUserId);
        ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId, long telegramUserId, string commandQuery, Type sessionObject);
        ISessionInfoWithData<TData> GetSessionInfoWithData<TData>(DateTime now, long chatId, long telegramUserId, string commandQuery);
        Task CreateSession(ISessionInfoWithData getCommandQuery);
        Task UpdateSession(ISessionInfoWithData session, long chatIdFrom);
    }
}