using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Core
{
    public interface ITelegramCommandFactory
    {
        Task<ITelegramCommand<T>> GetCommand<T>(T message, Type commandType);
    }
}