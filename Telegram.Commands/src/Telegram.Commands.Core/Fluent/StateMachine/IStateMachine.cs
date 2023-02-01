namespace Telegram.Commands.Core.Fluent.StateMachine;

public interface IStateMachine<TObj>
{
    IStateBase<TObj> GetState(string currentStateId);
}