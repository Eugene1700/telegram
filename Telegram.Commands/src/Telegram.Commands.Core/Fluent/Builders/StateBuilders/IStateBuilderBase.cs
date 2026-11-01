using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders
{
    public interface IStateBuilderBase<TObj, TStates>
    {
        IMessageBuilderBase<TObj, TStates> WithMessage<TQuery>(Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<QueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender);
    }
}