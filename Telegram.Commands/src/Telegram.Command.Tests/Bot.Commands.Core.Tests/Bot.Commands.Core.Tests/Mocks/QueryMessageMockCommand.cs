using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Tests.Mocks;

[Command(Name = "querymessagemock")]
public class QueryMessageMockCommand: IQueryCommand<Message>
{
    private ICommandExecutionResult _result = null;
    private Action<IQueryContext<Message>> _messageCallback = null!;

    public void SetResult(ICommandExecutionResult result, Action<IQueryContext<Message>> messageCallback = null)
    {
        _result = result;
        _messageCallback = messageCallback;
    }

    public Task<ICommandExecutionResult> Execute(IQueryContext<Message> query)
    {
        _messageCallback?.Invoke(query);
        return Task.FromResult(_result);
    }
}