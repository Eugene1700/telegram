using System;

namespace Bot.Commands.Abstract.Interfaces
{

    public interface ISessionInfoWithData<out T> : ISessionInfo
    {
        public T Data { get; }
    }
}