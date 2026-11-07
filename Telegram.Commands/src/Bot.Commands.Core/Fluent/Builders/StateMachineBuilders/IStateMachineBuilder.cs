using Bot.Commands.Abstract.Commands;
using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;

namespace Bot.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBuilder<TObj, TStates>: IStateMachineBaseBuilder<TObj, TStates>
    {
        IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null);
        IStateBuilder<TObj, TStates> State(TStates stateId, Func<IQueryContext<object>, TStates, TObj, IBotMessage[], Task> sender, uint? durationInSec = null);
    }
}