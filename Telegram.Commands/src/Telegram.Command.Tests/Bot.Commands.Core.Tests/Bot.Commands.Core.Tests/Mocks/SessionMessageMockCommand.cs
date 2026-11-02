using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Tests.Mocks;

[Command(Name = "sessionmessagemock_session")]
public class SessionMessageMockCommand: ISessionCommand<Message, SessionObjectMock>
{

    private ICommandExecutionResult _result = null;
    private Action<QueryContext<Message>, SessionObjectMock> _messageCallback = null!;

    public void SetResult(ICommandExecutionResult result, Action<QueryContext<Message>, SessionObjectMock> messageCallback = null)
    {
        _result = result;
        _messageCallback = messageCallback;
    }

    public Task<ICommandExecutionResult> Execute(QueryContext<Message> query, SessionObjectMock sessionObject)
    {
        _messageCallback?.Invoke(query, sessionObject);
        return Task.FromResult(_result);
    }
}