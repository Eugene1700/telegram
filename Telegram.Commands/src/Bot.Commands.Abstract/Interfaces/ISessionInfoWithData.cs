using System;

namespace Bot.Commands.Abstract.Interfaces
{

    public interface ISessionInfoWithData : ISessionInfo
    {
        public object Data { get; }
    }
}