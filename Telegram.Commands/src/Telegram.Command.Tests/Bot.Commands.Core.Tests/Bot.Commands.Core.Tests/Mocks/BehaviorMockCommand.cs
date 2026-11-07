using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Core.Tests.Mocks;

[Command(Name = "behaviormock_command")]
public class BehaviorMockCommand: IBehaviorCommand<SessionObjectMock>
{
    private ICommandExecutionResult _result = null;
    private Action<object, SessionObjectMock> _defaultExecuteCallback = null!;
    private Action<object,object,SessionObjectMock> _sessionCommandExecuteCallback;
    private ICommandExecutionResult _sessionResult;
    private Action<object,object,SessionObjectMock> _queryCommandExecuteCallback;
    private ICommandExecutionResult _queryResult;

    public void SetDefaultExecuteResult(ICommandExecutionResult result, Action<object, SessionObjectMock> defaultExecuteCallback = null)
    {
        _result = result;
        _defaultExecuteCallback = defaultExecuteCallback;
    }

    public void SetSessionCommandExecuteResult(ICommandExecutionResult result, Action<object, object, SessionObjectMock> sessionCommandExecuteCallback)
    {
        _sessionResult = result;
        _sessionCommandExecuteCallback = sessionCommandExecuteCallback;
    }

    public void SetQueryCommandExecuteResult(ICommandExecutionResult result, Action<object, object, SessionObjectMock> queryCommandExecuteCallback)
    {
        _queryResult = result;
        _queryCommandExecuteCallback = queryCommandExecuteCallback;
    }

    public Task<ICommandExecutionResult> DefaultExecute<TQuery>(IQueryContext<TQuery> query, SessionObjectMock sessionObject)
    {
        _defaultExecuteCallback?.Invoke(query, sessionObject);
        return Task.FromResult(_result);
    }

    public Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> currentCommand, IQueryContext<TQuery> query, SessionObjectMock sessionObject)
    {
        _queryCommandExecuteCallback?.Invoke(currentCommand, query, sessionObject);
        return Task.FromResult(_queryResult);
    }

    public Task<ICommandExecutionResult> Execute<TQuery>(ISessionCommand<TQuery, SessionObjectMock> currentCommand, IQueryContext<TQuery> query, SessionObjectMock sessionObject)
    {
        _sessionCommandExecuteCallback?.Invoke(currentCommand, query, sessionObject);
        return Task.FromResult(_sessionResult);
    }
}