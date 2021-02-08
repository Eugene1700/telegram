using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionsStore
    {
        ISessionInfo GetSessionInfo(DateTime now, long chatId, long telegramUserId);
        Task CreateSession<TData>(CommandSession<TData> getCommandQuery);
        Task UpdateSession<TData>(CommandSession<TData> session);
    }
}