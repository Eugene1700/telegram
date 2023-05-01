using System.Threading.Tasks;

namespace Telegram.Commands.Core.Fluent
{
    public interface IMessageSender<in TObj>
    {
        Task Send<TQuery>(TQuery currentQuery, TObj obj, ITelegramMessage message);
    }
    
    public interface IMessagesSender<in TObj>
    {
        Task Send<TQuery>(TQuery currentQuery, TObj obj, ITelegramMessage[] message);
    }
}