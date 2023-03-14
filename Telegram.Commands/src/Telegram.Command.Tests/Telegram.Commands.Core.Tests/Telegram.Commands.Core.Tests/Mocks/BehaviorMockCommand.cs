using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Tests.Mocks;

[Command(Name = "behaviormock_command")]
public class BehaviorMockCommand: IBehaviorTelegramCommand<SessionObjectMock>
{
    private ITelegramCommandExecutionResult _result = null;
    private Action<object, SessionObjectMock> _defaultExecuteCallback = null!;
    private Action<object,object,SessionObjectMock> _sessionCommandExecuteCallback;
    private ITelegramCommandExecutionResult _sessionResult;
    private Action<object,object,SessionObjectMock> _queryCommandExecuteCallback;
    private ITelegramCommandExecutionResult _queryResult;

    public void SetDefaultExecuteResult(ITelegramCommandExecutionResult result, Action<object, SessionObjectMock> defaultExecuteCallback = null)
    {
        _result = result;
        _defaultExecuteCallback = defaultExecuteCallback;
    }
    
    public Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query, SessionObjectMock sessionObject)
    {
        _defaultExecuteCallback?.Invoke(query, sessionObject);
        return Task.FromResult(_result);
    }

    public Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand, TQuery query, SessionObjectMock sessionObject)
    {
        _queryCommandExecuteCallback?.Invoke(currentCommand, query, sessionObject);
        return Task.FromResult(_queryResult);
    }

    public Task<ITelegramCommandExecutionResult> Execute<TQuery>(ISessionTelegramCommand<TQuery, SessionObjectMock> currentCommand, TQuery query, SessionObjectMock sessionObject)
    {
        _sessionCommandExecuteCallback?.Invoke(currentCommand, query, sessionObject);
        return Task.FromResult(_sessionResult);
    }

    public void SetSessionCommandExecuteResult(ITelegramCommandExecutionResult result, Action<object, object, SessionObjectMock> sessionCommandExecuteCallback)
    {
        _sessionResult = result;
        _sessionCommandExecuteCallback = sessionCommandExecuteCallback;
    }

    public void SetQueryCommandExecuteResult(ITelegramCommandExecutionResult result, Action<object, object, SessionObjectMock> queryCommandExecuteCallback)
    {
        _queryResult = result;
        _queryCommandExecuteCallback = queryCommandExecuteCallback;
    }
}