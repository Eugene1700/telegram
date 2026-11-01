using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders
{
    public interface IStateBuilder<TObj, TStates>
    {
        IStateBase<TStates> GetState();

        IStateMachineBuilder<TObj, TStates> Next<TQuery>(Func<TQuery, TStates, TObj, Task<TStates>> handler, bool force)
            where TQuery : class;

        IStateMachineBuilder<TObj, TStates> Next(TStates stateId, bool force);
        IStateMachineBuilder<TObj, TStates> Loop(bool force);
        IStateMachineBuilder<TObj, TStates> FireAndForget();

        IMessageBuilder<TObj, TStates> WithMessage<TQuery>(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<QueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender = null);

        IStateBuilder<TObj, TStates> WithMessages(
            Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider);
    }
    
    
}