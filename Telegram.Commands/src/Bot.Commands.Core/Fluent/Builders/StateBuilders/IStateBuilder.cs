using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;

namespace Bot.Commands.Core.Fluent.Builders.StateBuilders
{
    public interface IStateBuilder<TObj, TStates>
    {
        IStateBase<TStates> GetState();

        IStateMachineBuilder<TObj, TStates> Next<TQuery>(Func<IQueryContext<TQuery>, TStates, TObj, Task<TStates>> handler, bool force)
            where TQuery : class;

        IStateMachineBuilder<TObj, TStates> Next(TStates stateId, bool force);
        IStateMachineBuilder<TObj, TStates> Loop(bool force);
        IStateMachineBuilder<TObj, TStates> FireAndForget();

        IMessageBuilder<TObj, TStates> WithMessage<TQuery>(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<IQueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender = null);

        IMessageBuilder<TObj, TStates> WithMessage(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<IQueryContext<object>, TStates, TObj, IBotMessage, Task> sender);

        IStateBuilder<TObj, TStates> WithMessages(
            Func<TStates, TObj, IStateBuilderBase<TObj, TStates>, Task> messageFlowProvider);
    }
    
    
}
