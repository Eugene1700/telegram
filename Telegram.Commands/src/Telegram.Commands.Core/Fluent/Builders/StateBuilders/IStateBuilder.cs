using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders;

public interface IStateBuilder<TObj, TStates, TCallbacks>
{
    IMessageBuilder<TObj, TStates, TCallbacks> WithMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sender);
}