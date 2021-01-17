using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Sessions
{
    public interface ISessionStore
    {
        long Save<TData>(Session<TData> sessionObj);
        Session<TData> Get<TData>(long chatId, string commandId);
        void Update<TData>(Session<TData> sessionObj);
    }
}