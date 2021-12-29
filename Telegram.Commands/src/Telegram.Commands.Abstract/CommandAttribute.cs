using System;

namespace Telegram.Commands.Abstract
{
    public class CommandAttribute : Attribute, ITelegramCommandDescriptor
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

    public interface ITelegramCommandDescriptor
    {
        string Name { get; }
        public ChatArea Area { get; }
        public string[] Swarms { get; set; }
    }
}