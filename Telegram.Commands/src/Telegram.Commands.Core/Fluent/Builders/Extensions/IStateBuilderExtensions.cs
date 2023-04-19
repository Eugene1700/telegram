using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions;

public static class IStateBuilderExtensions
{
    public static IMessageBuilder<TObj> WithMessage<TObj>(this IStateBuilder<TObj> stateBuilder, string message,
        IMessageSender<TObj> sender)
    {
        return stateBuilder.WithMessage(_ => Task.FromResult(message), sender);
    }
}