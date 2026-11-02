namespace Bot.Commands.Abstract.Messages
{
    public interface ICallbackMessage<out TReplyMarkup>: ITextMessage
    {
        TReplyMarkup ReplyMarkup { get; }
    }
}