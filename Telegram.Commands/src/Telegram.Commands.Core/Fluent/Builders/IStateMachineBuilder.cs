using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateMachineBuilder<TObj>
{
    IStateBuilder<TObj> NewState(string message);
    IStateMachine<TObj> Finish();
}