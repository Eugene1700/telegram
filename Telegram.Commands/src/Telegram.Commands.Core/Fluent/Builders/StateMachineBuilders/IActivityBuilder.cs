using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders
{
    public interface IActivityPrimaryBuilder<TObj, TStates>: IActivityBuilder<TObj, TStates>
    {
        IActivityBuilder<TObj, TStates> WithTransport(Func<object, TStates, TObj, ITelegramMessage, Task> sender);
    }
    
    public interface IActivityBuilder<TObj, TStates>: IActivityBaseBuilder<TObj, TStates>
    {
        IStateBuilder<TObj, TStates> State(TStates stateId, uint? durationInSec = null);
        IStateBuilder<TObj, TStates> State(TStates stateId, Func<object, TStates, TObj, ITelegramMessage, Task> sender, uint? durationInSec = null);
    }
}