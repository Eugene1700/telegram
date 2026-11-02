using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Core.Fluent.Builders.StateBuilders
{
    public interface IStateBuilderBase<TObj, TStates>
    {
        IMessageBuilderBase<TObj, TStates> WithMessage(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<object, TStates, TObj, IBotMessage, Task> sender);
        IMessageBuilderBase<TObj, TStates> WithMessage<TQuery>(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<QueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender);
    }
}
