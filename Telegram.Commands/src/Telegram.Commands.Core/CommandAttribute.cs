using System;
using Telegram.Commands.Abstract;

namespace Telegram.Commands.Core
{
    public class CommandAttribute : Attribute, ITelegramCommandDescriptor
    {
        public string Name { get; set; }
        public Permissions Permission { get; set; }
        public CommandChain Chain { get; set; }
        public string NextCommandName { get; set; }

        public CommandAttribute()
        {
            Chain = CommandChain.None;
            Permission = Permissions.User;
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
        string NextCommandName { get; }
    }
}