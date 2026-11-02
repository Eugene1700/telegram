using System;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface ICommandFactory
    {
        object GetCommand(Type commandType);
    }
}