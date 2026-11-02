namespace Bot.Commands.Abstract.Interfaces
{
    public interface ICommandExecutionResult
    {
        public object Data { get;}
        public ICommandDescriptor NextCommandDescriptor { get;}
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