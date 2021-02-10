using System;

namespace Telegram.Commands.Abstract
{
    public class CommandAttribute : Attribute, ITelegramCommandDescriptor
    {
        public string Name { get; set; }
        public Permissions Permission { get; set; }
        public CommandChain Chain { get; set; }

        public CommandAttribute()
        {
            Permission = Permissions.User;
            Chain = CommandChain.None;
        }
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
    }
}