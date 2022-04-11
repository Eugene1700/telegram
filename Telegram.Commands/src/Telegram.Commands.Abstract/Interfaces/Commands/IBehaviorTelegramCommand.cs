using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces.Commands
{
    public interface IBehaviorTelegramCommand<TQueryData, TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> Execute<TQuery>(IBaseCommand<TQuery, TQueryData, TSessionObject> command,
            TQueryData queryData, TSessionObject sessionObject);
        
    }
}