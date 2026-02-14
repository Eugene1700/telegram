using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Messages;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBuilder<TObj, TStates>: IStateMachineBaseBuilder<TObj, TStates>
    {
        IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null);
        IStateBuilder<TObj, TStates> State(TStates stateId, Func<object, TStates, TObj, ITelegramMessage[], Task> sender, uint? durationInSec = null);
    }
}