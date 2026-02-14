using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract.Commands
{
    public interface IQueryTelegramCommand<in TQuery>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query);
    }
}