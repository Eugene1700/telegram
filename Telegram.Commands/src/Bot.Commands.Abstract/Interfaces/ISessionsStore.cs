using System;
using System.Threading.Tasks;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface ISessionsStore
    {
        ISessionInfo GetSessionInfo(DateTime now, long chatId, long userId);
        ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId, long userId, string commandQuery, Type sessionObject);
        ISessionInfoWithData<TData> GetSessionInfoWithData<TData>(DateTime now, long chatId, long userId, string commandQuery);
        Task CreateSession(ISessionInfoWithData getCommandQuery);
        Task UpdateSession(ISessionInfoWithData session, long chatIdFrom);
    }
}