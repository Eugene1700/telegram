using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommand<in TQuery>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query);
    }
    
    public interface ISessionTelegramCommand<in TQuery, in TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query, TSessionObject sessionObject);
    }
}