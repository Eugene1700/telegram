using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions;

public static class IStateBuilderExtensions
{
    public static IMessageBuilder<TObj, TStates, TCallbacks> WithMessage<TObj, TStates, TCallbacks>(this IStateBuilder<TObj, TStates, TCallbacks> stateBuilder, string message,
        IMessageSender<TObj> sender)
    {
        return stateBuilder.WithMessage(_ => Task.FromResult(message), sender);
    }
}