using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IMessageBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> WithCallbacks();
}