using System;
using System.Threading.Tasks;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;

namespace Bot.Commands.Core.Fluent.Builders.CallbackBuilders
{

    public interface ICallbacksBuilderForMessage<TObj, TStates>: IStateBuilderBase<TObj, TStates>
    {
        ICallbackRowBuilderForMessage<TObj, TStates> Row();
        ICallbacksBuilderForMessage<TObj, TStates> KeyBoard(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider);
    }
}