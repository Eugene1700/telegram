using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Tests.Mocks;

[Command(Name = "sessionmessagemockwithemptyobject_session")]
public class SessionMessageWithEmptyObjectMockCommand: ISessionCommand<Message, EmptyObject>
{
    private ICommandExecutionResult _result = null;
    private Action<IQueryContext<Message>, EmptyObject> _messageCallback = null!;

    public void SetResult(ICommandExecutionResult result, Action<IQueryContext<Message>, EmptyObject> messageCallback = null)
    {
        _result = result;
        _messageCallback = messageCallback;
    }

    public Task<ICommandExecutionResult> Execute(IQueryContext<Message> query, EmptyObject sessionObject)
    {
        _messageCallback?.Invoke(query, sessionObject);
        return Task.FromResult(_result);
    }
}