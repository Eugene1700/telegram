using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders;

public interface IStateBuilderBase<TObj>
{
    IStateBase<TObj> GetState();
    IStateMachineBodyBuilder<TObj> NextState<TQuery>(Func<TQuery, TObj, Task<string>> commitStateExpr) where TQuery : class;
    IStateMachineBodyBuilder<TObj> NextState(string stateId);
}