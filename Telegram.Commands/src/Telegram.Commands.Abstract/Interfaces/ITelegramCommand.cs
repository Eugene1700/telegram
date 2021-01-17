using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommand<in T>
    {
        Task Execute(T query);
    }
}