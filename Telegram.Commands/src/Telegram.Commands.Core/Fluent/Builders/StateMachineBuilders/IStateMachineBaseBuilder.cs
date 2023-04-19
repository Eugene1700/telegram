using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;

public interface IStateMachineBaseBuilder<TObj>
{
    IStateMachine<TObj> Build();
    IStateMachineBodyBuilder<TObj> Exit<TQuery>(string stateId,
        Func<TQuery, TObj, Task<ITelegramCommandExecutionResult>> finalizer) where TQuery: class;
}