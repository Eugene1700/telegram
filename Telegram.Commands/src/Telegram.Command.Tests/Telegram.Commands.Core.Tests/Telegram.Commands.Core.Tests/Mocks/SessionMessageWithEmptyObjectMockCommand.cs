using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Tests.Mocks;

[Command(Name = "sessionmessagemockwithemptyobject_session")]
public class SessionMessageWithEmptyObjectMockCommand: ISessionTelegramCommand<Message, EmptyObject>
{
    private ITelegramCommandExecutionResult _result = null;
    private Action<Message, EmptyObject> _messageCallback = null!;

    public void SetResult(ITelegramCommandExecutionResult result, Action<Message, EmptyObject> messageCallback = null)
    {
        _result = result;
        _messageCallback = messageCallback;
    }
    
    public Task<ITelegramCommandExecutionResult> Execute(Message query, EmptyObject sessionObject)
    {
        _messageCallback?.Invoke(query, sessionObject);
        return Task.FromResult(_result);
    }
}