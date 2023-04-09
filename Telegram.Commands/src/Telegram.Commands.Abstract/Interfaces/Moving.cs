namespace Telegram.Commands.Abstract.Interfaces;

/// <summary>
/// Moving to the next state
/// </summary>
public enum Moving
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