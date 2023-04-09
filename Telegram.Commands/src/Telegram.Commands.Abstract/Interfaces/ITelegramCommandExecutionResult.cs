namespace Telegram.Commands.Abstract.Interfaces
{
    /// <summary>
    /// Moving descriptor after command execution
    /// </summary>
    public interface ITelegramCommandExecutionResult
    {
        /// <summary>
        /// Data which will be moved to the next state
        /// </summary>
        public object Data { get;}
        
        /// <summary>
        /// Next state descriptor
        /// </summary>
        public ITelegramCommandDescriptor NextCommandDescriptor { get;}
        
        /// <summary>
        /// Moving
        /// </summary>
        public Moving Moving { get;}
        
        /// <summary>
        /// It is ID of chat, where session will wait some user actions
        /// </summary>
        public long? WaitFromChatId { get; }
        
        /// <summary>
        /// Duration of next state session
        /// </summary>
        public uint? SessionDurationInSec { get; }
    }
}