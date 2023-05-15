using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IStateMachineBuilder<TObj, TStates>: IStateMachineBaseBuilder<TObj, TStates>
    {
        IStateBuilder<TObj, TStates> Entry(TStates stateId, uint? durationInSec = null);
        IStateBuilder<TObj, TStates> Entry(TStates stateId, Func<object, TObj, ITelegramMessage[], Task> obj, uint? durationInSec = null);
    }
}