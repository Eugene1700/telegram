using System;
using Bot.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class SystemClockService : IClock, EntityStorage.IClock
    {
        public DateTime Now => DateTime.Now;
    }
}