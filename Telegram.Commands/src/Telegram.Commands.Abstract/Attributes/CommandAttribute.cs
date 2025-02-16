using System;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract.Attributes
{
    public class CommandAttribute : Attribute, ITelegramCommandDescriptor
    {
        public string Name { get; set; }
        public bool Authorized { get; set; }
        
        /// <summary>
        /// Call behavior command without session
        /// </summary>
        public bool DirectCallAllowed { get; set; }
        public ChatArea Area { get; set; }
        public string[] Swarms { get; set; }

        public CommandAttribute()
        {
            Authorized = true;
            Area = ChatArea.Private;
            DirectCallAllowed = false;
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