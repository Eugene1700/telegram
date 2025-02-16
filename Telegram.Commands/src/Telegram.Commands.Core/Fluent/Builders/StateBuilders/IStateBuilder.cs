using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders
{
    public interface IStateBuilder<TObj, TStates>
    {
        IStateBase<TStates> GetState();

        IActivityBuilder<TObj, TStates> Next<TQuery>(Func<TQuery, TStates, TObj, Task<TStates>> handler,
            bool forceRerender = true)
            where TQuery : class;

        IActivityBuilder<TObj, TStates> Next(TStates stateId, bool forceRerender = true);
        IActivityBuilder<TObj, TStates> Loop(bool forceRerender = true);
        IActivityBuilder<TObj, TStates> FireAndForget();

        IMessageBuilder<TObj, TStates> WithMessage(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender = null);

        IStateBuilder<TObj, TStates> WithMessages(
            Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider);
    }
}