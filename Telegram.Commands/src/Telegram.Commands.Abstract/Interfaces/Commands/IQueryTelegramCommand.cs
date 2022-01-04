using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces.Commands
{
    public interface IQueryTelegramCommand<in TQuery>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query);
    }
}