namespace Bot.Commands.Abstract.Messages
{
    public record CallbackData
    {
        public string Text { get; init; }
        public string CallbackText { get; init; }
        public CallbackMode CallbackMode { get; init; }
    }
}