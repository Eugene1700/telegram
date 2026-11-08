using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Routing;

namespace SimpleHandlers.Services.Commands.Routing;

public abstract class Flow<TData, TState>(ISampleMessageSender sender) : RoutedFluentCommand<TData, TState>
    where TData : RoutedData<TState>
{
    protected Task Send(IQueryContext<object> query, TState state, TData data, IBotMessage message)
        => sender.Send(query.GetChatId(), message);
}
