using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbacksBuilderBase<TObj, TStates, TCallbacks>
{
    ICallbackRowBuilderBase<TObj, TStates, TCallbacks> Row();
    ICallbacksBuilderBase<TObj, TStates, TCallbacks> Keyboard(Func<TObj, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task> provider);
}