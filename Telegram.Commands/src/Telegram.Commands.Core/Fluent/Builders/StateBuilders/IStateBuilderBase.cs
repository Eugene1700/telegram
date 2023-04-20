using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders;

public interface IStateBuilderBase<TObj, TStates, TCallbacks>
{
    IStateBase<TStates> GetState();
    IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next<TQuery>(Func<TQuery, TObj, Task<TStates>> handler) where TQuery : class;
    IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next(TStates stateId);
    IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Loop();
}