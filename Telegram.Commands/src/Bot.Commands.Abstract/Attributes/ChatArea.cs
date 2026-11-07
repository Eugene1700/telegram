using System;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Abstract.Attributes
{

    [Flags]
    public enum ChatArea
    {
        Private = 1,
        Group = 2,
        Channel = 4,
        SuperGroup = 8
    }
}