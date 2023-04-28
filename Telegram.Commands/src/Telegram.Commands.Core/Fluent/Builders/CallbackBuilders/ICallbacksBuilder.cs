using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbacksBuilder<TObj, TStates, TCallbacks> : IStateBuilder<TObj, TStates, TCallbacks>
    {
        ICallbackRowBuilder<TObj, TStates, TCallbacks> Row();
        ICallbacksBuilder<TObj, TStates, TCallbacks> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider);
    }

    public interface ICallbacksBuilderForMessage<TObj, TStates, TCallbacks>: IStateBuilderBase<TObj, TStates, TCallbacks>
    {
        ICallbackRowBuilderForMessage<TObj, TStates, TCallbacks> Row();
        ICallbacksBuilderForMessage<TObj, TStates, TCallbacks> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider);
    }
}