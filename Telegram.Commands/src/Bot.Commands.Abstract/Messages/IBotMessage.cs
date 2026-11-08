namespace Bot.Commands.Abstract.Messages
{
    public interface IBotMessage : ITextMessage
    {
        public CallbackDataWithCommand[][] Callbacks { get; }
    }
}