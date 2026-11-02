using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Tests.Mocks;

[Command(Name = "querymessagemock")]
public class QueryMessageMockCommand: IQueryCommand<Message>
{
    private ICommandExecutionResult _result = null;
    private Action<QueryContext<Message>> _messageCallback = null!;

    public void SetResult(ICommandExecutionResult result, Action<QueryContext<Message>> messageCallback = null)
    {
        _result = result;
        _messageCallback = messageCallback;
    }

    public Task<ICommandExecutionResult> Execute(QueryContext<Message> query)
    {
        _messageCallback?.Invoke(query);
        return Task.FromResult(_result);
    }
}