namespace Telegram.Commands.Core.Fluent.StateMachine;

public interface IStateBase<T>
{
    string Id { get; }
    uint? DurationInSec { get; }
}