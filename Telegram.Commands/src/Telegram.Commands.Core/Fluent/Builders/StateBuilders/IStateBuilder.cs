using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders
{
    public interface IStateBuilder<TObj, TStates, TCallbacks>
    {
        IStateBase<TStates> GetState();
        IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next<TQuery>(Func<TQuery, TObj, Task<TStates>> handler, bool force) where TQuery : class;
        IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Next(TStates stateId, bool force);
        IStateMachineBodyBuilder<TObj, TStates, TCallbacks> Loop(bool force);
        IMessageBuilder<TObj, TStates, TCallbacks> WithMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task> sender = null);
        IStateBuilder<TObj, TStates, TCallbacks> WithMessages(Func<TObj, IStateBuilderBase<TObj, TStates, TCallbacks>, Task> messageFlowProvider);
    }

    public interface IStateBuilderBase<TObj, TStates, TCallbacks>
    {
        IMessageBuilderBase<TObj, TStates, TCallbacks> WithMessage(Func<TObj, Task<string>> messageProvider, Func<object, TObj, ITelegramMessage, Task> sender = null);
    }
}