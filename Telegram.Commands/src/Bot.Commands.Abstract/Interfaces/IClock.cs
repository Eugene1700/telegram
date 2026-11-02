using System;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface IClock
    {
        DateTime Now { get; }
    }
}