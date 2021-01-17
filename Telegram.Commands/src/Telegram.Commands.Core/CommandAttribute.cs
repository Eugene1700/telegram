using System;
using Telegram.Commands.Abstract;

namespace Telegram.Commands.Core
{
    public class CommandAttribute : Attribute, ITelegramCommandDescriptor
    {
        public string Name { get; set; }
        public Permissions Permission { get; set; }
    }

    public interface ITelegramCommandDescriptor
    {
        string Name { get; set; }
        Permissions Permission { get; set; }
    }
}