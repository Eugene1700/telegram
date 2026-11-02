using System;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;

namespace Bot.Commands.Core.Fluent.Builders
{
    public interface IMessageBuilder<TObj, TStates> : IStateBuilder<TObj, TStates>
    {
        ICallbacksBuilder<TObj, TStates> WithCallbacks();
    }

    public interface IMessageBuilderBase<TObj, TStates>: IStateBuilderBase<TObj, TStates>
    {
        ICallbacksBuilderForMessage<TObj, TStates> WithCallbacks();
    }
}