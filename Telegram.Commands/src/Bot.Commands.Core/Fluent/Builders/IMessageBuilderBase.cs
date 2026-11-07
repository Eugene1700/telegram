using System;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;

namespace Bot.Commands.Core.Fluent.Builders
{

    public interface IMessageBuilderBase<TObj, TStates>: IStateBuilderBase<TObj, TStates>
    {
        ICallbacksBuilderForMessage<TObj, TStates> WithCallbacks();
    }
}