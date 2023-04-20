using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IMessageBuilder<TObj, TStates, TCallbacks> : IStateBuilderBase<TObj, TStates, TCallbacks>
{
    ICallbacksBuilder<TObj, TStates, TCallbacks> WithCallbacks();
}