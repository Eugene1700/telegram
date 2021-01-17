using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core
{
    public interface ITelegramCommandFactory
    {
        Task<ITelegramCommand<T>> GetCommand<T>(T message, Type commandType);
    }
}