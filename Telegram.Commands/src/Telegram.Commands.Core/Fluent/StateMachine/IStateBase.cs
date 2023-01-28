namespace Telegram.Commands.Core.Fluent.StateMachine;

public interface IStateBase<T>
{
    int Id { get; }
    ITelegramMessage GetMessage();
}