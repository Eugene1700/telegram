namespace Telegram.Commands.Core.Fluent.StateMachine;

public interface IStateMachine<TObj>
{
    IStateBase<TObj> GetState(int currentStateId);
}