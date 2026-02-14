using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract.Commands
{
    public interface ISessionTelegramCommand<in TQuery, in TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query, TSessionObject sessionObject);
    }
}