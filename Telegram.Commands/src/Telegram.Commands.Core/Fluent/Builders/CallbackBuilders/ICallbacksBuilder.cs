using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbacksBuilder<TObj, TStates, TCallbacks> : IStateBuilderBase<TObj, TStates, TCallbacks>
{
    ICallbackRowBuilder<TObj, TStates, TCallbacks> Row();
    ICallbacksBuilder<TObj, TStates, TCallbacks> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider);
}