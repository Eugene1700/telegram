using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionsStore
    {
        ISessionInfo GetSessionInfo(DateTime now, long chatId, long telegramUserId);
        Task CreateSession(ISessionInfo getCommandQuery);
        Task UpdateSession(ISessionInfo session);
    }
}