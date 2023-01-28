using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateBuilderBase<TObj>
{
    IStateBase<TObj> GetState();
    IStateMoverBuilder<TObj> ExitState(Func<string, TObj, Task<string>> commitStateExpr);
    IStateMoverBuilder<TObj> ExitState(string condition);
}