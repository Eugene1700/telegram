using System;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Abstract.Attributes
{
    public class CommandAttribute : Attribute, ICommandDescriptor
    {
        public string Name { get; set; }
        public bool Authorized { get; set; }
        public ChatArea Area { get; set; }
        public string[] Swarms { get; set; }

        public CommandAttribute()
        {
            Authorized = true;
            Area = ChatArea.Private;
        }
    }

    [Flags]
    public enum ChatArea
    {
        Private = 1,
        Group = 2,
        Channel = 4,
        SuperGroup = 8
    }
}