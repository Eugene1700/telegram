using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Core.Fluent.Builders.StateBuilders;

public interface IStateBuilder<TObj>
{
    IMessageBuilder<TObj> WithMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sender);
}