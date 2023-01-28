using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateMoverBuilder<TObj>
{
    IStateMoverBuilder<TObj> ConditionNext(string condition, IStateBase<TObj> nextState);
    IStateMoverBuilder<TObj> Loop(string condition);
    IStateMoverBuilder<TObj> Next(IStateBase<TObj> nextState);
    IStateMachineBuilder<TObj> Finish();
}