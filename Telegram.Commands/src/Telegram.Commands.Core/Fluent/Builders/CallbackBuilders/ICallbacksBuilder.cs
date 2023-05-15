using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbacksBuilder<TObj, TStates> : IStateBuilder<TObj, TStates>
    {
        ICallbackRowBuilder<TObj, TStates> Row();
        ICallbacksBuilder<TObj, TStates> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider);
    }

    public interface ICallbacksBuilderForMessage<TObj, TStates>: IStateBuilderBase<TObj, TStates>
    {
        ICallbackRowBuilderForMessage<TObj, TStates> Row();
        ICallbacksBuilderForMessage<TObj, TStates> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider);
    }
}