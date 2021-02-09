using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionsStore
    {
        ISessionInfo GetSessionInfo(DateTime now, long chatId, long telegramUserId);
        Task CreateSession(CommandSession getCommandQuery);
        Task UpdateSession(CommandSession session);
    }
}