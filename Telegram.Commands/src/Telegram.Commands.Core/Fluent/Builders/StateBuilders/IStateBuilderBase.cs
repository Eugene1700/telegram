using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders
{

    public interface IStateBuilderBase<TObj, TStates>
    {
        IMessageBuilderBase<TObj, TStates> WithMessage(Func<TStates, TObj, Task<string>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender = null);
    }
}