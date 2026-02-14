namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramCommandExecutionResult
    {
        public object Data { get;}
        public ITelegramCommandDescriptor NextCommandDescriptor { get;}
        public ExecuteResult Result { get;}
        public long? WaitFromChatId { get; }
        public uint? SessionDurationInSec { get; }
    }

    public enum ExecuteResult
    {
        Freeze,
        Ahead,
        Break,
        Fire
    }
}