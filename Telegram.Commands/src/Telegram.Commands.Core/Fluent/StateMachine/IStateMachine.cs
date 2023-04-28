namespace Telegram.Commands.Core.Fluent.StateMachine
{
    public interface IStateMachine<TStates>
    {
        IStateBase<TStates> GetState(TStates currentStateId);
    }
}