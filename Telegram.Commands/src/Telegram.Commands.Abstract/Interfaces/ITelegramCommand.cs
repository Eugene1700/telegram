using System.Threading.Tasks;

namespace Telegram.Commands.Core
{
    public interface ITelegramCommand<in T>
    {
        Task Execute(T query);
    }
}