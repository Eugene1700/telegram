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
        public ExecuteResult Result { get;}
        
        /// <summary>
        /// It is ID of chat, where session will wait some user actions
        /// </summary>
        public long? WaitFromChatId { get; }
        
        /// <summary>
        /// Duration of next state session
        /// </summary>
        public uint? SessionDurationInSec { get; }
    }

    /// <summary>
    /// Moving to the next state
    /// </summary>
    public enum ExecuteResult
    {
        /// <summary>
        /// Stay in current state. Session will not be prolonged.
        /// </summary>
        Freeze,
        
        /// <summary>
        /// Move to the next state.
        /// </summary>
        Ahead,
        
        /// <summary>
        /// Chain of commands will be interrupted
        /// </summary>
        Break
    }
}