using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

public interface IStateMachineBodyBuilder<TObj> : IStateMachineBaseBuilder<TObj>
{
    IStateBuilder<TObj> State(string stateId);
}