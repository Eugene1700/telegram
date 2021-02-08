using System;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IClock
    {
        DateTime Now { get; set; }
    }
}