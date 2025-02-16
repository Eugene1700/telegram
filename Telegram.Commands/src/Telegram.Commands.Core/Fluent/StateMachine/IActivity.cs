namespace Telegram.Commands.Core.Fluent.StateMachine
{
    public interface IActivity<TStates>
    {
        IStateBase<TStates> GetState(TStates currentStateId);
    }
}