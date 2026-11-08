using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Routing;
namespace SurveyComparison;
public abstract class Flow<TData, TState>(Output output) : RoutedFluentCommand<TData, TState>
    where TData : RoutedData<TState>
{
    protected Task Send(IQueryContext<object> query, TState state, TData data, IBotMessage message)
    { output.Show(message); return Task.CompletedTask; }
    protected void Show(string text) => output.Show(new BotMessage(text, null, null, null));
}
