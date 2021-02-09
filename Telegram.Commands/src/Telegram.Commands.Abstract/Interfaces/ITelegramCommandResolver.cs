using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommandResolver
    {
        Task<TelegramResult> Handle(Update update);
    }
}