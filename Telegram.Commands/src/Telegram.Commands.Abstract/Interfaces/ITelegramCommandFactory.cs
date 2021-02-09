using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommandFactory
    {
        Task<ITelegramCommand<T>> GetCommand<T>(T message, Type commandType);
    }
}