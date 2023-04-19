namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbacksBuilderBase<TObj>
{
    ICallbackRowBuilderBase<TObj> Row();
}