using System;

namespace Telegram.Commands.Abstract
{
    public class CommandAttribute : Attribute, ITelegramCommandDescriptor
    {
        public string Name { get; set; }
        public Permissions Permission { get; set; }

        public ChatArea Area { get; set; }
        
        public Type[] Reaction { get; set; }
        public Type[] By { get; set; }

        public CommandAttribute()
        {
            Permission = Permissions.User;
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
        Permissions Permission { get;}
        public ChatArea Area { get; }
        public Type[] Reaction { get; }
        public Type[] By { get; set; }
    }
}