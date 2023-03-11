using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Tests.Mocks;

[Command(Name = "sessionmessagemock_session")]
public class SessionMessageMockCommand: ISessionTelegramCommand<Message, SessionObjectMock>
{
    
    private ITelegramCommandExecutionResult _result = null;
    private Action<Message, SessionObjectMock> _messageCallback = null!;

    public void SetResult(ITelegramCommandExecutionResult result, Action<Message, SessionObjectMock> messageCallback = null)
    {
        _result = result;
        _messageCallback = messageCallback;
    }
    
    public Task<ITelegramCommandExecutionResult> Execute(Message query, SessionObjectMock sessionObject)
    {
        _messageCallback?.Invoke(query, sessionObject);
        return Task.FromResult(_result);
    }
}