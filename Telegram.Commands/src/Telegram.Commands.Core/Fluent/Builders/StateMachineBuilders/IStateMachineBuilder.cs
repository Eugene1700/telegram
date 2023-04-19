using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

public interface IStateMachineBuilder<TObj>: IStateMachineBaseBuilder<TObj>
{
    IStateBuilder<TObj> Entry(string stateId, uint? durationInSec = null);
}