namespace Bot.Commands.Abstract.Messages
{
    public record BotMessage(
        string Text,
        byte[] Photo,
        IMessageTextFormat Format,
        CallbackDataWithCommand[][] Callbacks) : IBotMessage
    {
        public bool SendAsFile { get; init; }
        public string FileName { get; init; }
    }
}