namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> WithCallbacks();
}