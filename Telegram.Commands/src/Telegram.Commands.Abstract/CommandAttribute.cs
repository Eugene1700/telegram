using System;

namespace Telegram.Commands.Abstract
{
    public class CommandAttribute : Attribute, ITelegramCommandDescriptor
    {
        public string Name { get; set; }
        public Permissions Permission { get; set; }
        public CommandChain Chain { get; set; }

        public ChatArea Area { get; set; }

        public CommandAttribute()
        {
            Permission = Permissions.User;
            Chain = CommandChain.None;
            Area = ChatArea.Private;
        }
    }

    [Flags]
    public enum ChatArea
    {
        Private,
        Group,
        Channel,
        SuperGroup
    }

    public enum CommandChain
    {
        None,
        StartPoint,
        TransitPoint,
        EndPoint
    }

    public interface ITelegramCommandDescriptor
    {
        string Name { get; }
        Permissions Permission { get;}
        CommandChain Chain { get; }
        public ChatArea Area { get; set; }
    }
}