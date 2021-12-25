using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ISessionTelegramCommand<in TQuery, in TSessionObject>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query, TSessionObject sessionObject);
    }

    public class EmptyObject
    {
    }
}