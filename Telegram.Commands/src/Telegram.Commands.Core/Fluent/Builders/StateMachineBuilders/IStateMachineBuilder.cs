using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBuilder<TObj, TStates>: IStateMachineBaseBuilder<TObj, TStates>
    {
        IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null);
        IStateBuilder<TObj, TStates> State(TStates stateId, Func<object, TObj, ITelegramMessage[], Task> sender, uint? durationInSec = null);
    }
}