namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbacksBuilderBase<TObj, TStates, TCallbacks>
{
    ICallbackRowBuilderBase<TObj, TStates, TCallbacks> Row();
}