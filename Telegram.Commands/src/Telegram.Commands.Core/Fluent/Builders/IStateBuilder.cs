using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateBuilder<TObj>
{
    IMessageBuilder<TObj> WithMessage(Func<TObj, string> messageProvider, Func<TObj, ITelegramMessage, Task> sendMessageProvider);
}

public interface IMessageBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> WithCallbacks();
}