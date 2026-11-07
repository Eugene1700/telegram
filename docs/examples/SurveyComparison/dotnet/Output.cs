using System.Text.Json;
using Bot.Commands.Abstract.Messages;

namespace SurveyComparison;

public sealed class Output
{
    public object Last { get; private set; }
    public void Show(IBotMessage message)
    {
        Last = new { text = message.Text, buttons = (message.Callbacks ?? Enumerable.Empty<CallbackDataWithCommand>())
            .Select(c => new { title = c.Text, data = c.CommandDescriptor == null ? c.CallbackText : $"/{c.CommandDescriptor.Name} {c.CallbackText}" }).ToArray() };
    }
    public void Flush() => Console.WriteLine(JsonSerializer.Serialize(Last));
}
