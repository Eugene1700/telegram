using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface IStateBuilder<TObj>
{
    IMessageBuilder<TObj> WithMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sender);
}

public interface IMessageSender<in TObj>
{
    Task Send<TQuery>(TQuery currentQuery, TObj obj, ITelegramMessage message);
}

public interface IMessageBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> WithCallbacks();
}