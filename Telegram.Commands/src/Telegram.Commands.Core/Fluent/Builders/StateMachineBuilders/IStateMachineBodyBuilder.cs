using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBodyBuilder<TObj, TStates, TCallbacks> : IStateMachineBaseBuilder<TObj, TStates, TCallbacks>
    {
        IStateBuilder<TObj, TStates, TCallbacks> State(TStates stateId);
    }
}