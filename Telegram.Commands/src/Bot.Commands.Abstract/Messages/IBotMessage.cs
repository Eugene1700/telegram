using System.Collections.ObjectModel;

namespace Bot.Commands.Abstract.Messages
{

    public interface IBotMessage : ITextMessage
    {
        public ReadOnlyCollection<CallbackDataWithCommand> Callbacks { get;}
    }
}