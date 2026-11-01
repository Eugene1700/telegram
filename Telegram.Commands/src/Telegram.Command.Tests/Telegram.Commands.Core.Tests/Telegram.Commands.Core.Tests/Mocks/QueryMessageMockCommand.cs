using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Tests.Mocks;

[Command(Name = "querymessagemock")]
public class QueryMessageMockCommand: IQueryTelegramCommand<Message>
{
    private ITelegramCommandExecutionResult _result = null;
    private Action<QueryContext<Message>> _messageCallback = null!;

    public void SetResult(ITelegramCommandExecutionResult result, Action<QueryContext<Message>> messageCallback = null)
    {
        _result = result;
        _messageCallback = messageCallback;
    }

    public Task<ITelegramCommandExecutionResult> Execute(QueryContext<Message> query)
    {
        _messageCallback?.Invoke(query);
        return Task.FromResult(_result);
    }
}