namespace Telegram.Commands.Core.Fluent.Builders;

public interface ICallbacksBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbackRowBuilder<TObj> Row();
}