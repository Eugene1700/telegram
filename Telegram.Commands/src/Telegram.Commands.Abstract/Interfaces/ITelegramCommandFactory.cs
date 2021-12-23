using System;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommandFactory
    {
        object GetCommand(Type commandType);
    }
}