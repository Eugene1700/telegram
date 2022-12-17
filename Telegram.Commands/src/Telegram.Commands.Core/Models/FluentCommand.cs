using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Models;

public abstract class FluentCommand<TObject>: IBehaviorTelegramCommand<FluentObject<TObject>>
{
    public Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query, FluentObject<TObject> sessionObject)
    {
        throw new System.NotImplementedException();
    }

    public Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand, TQuery query, FluentObject<TObject> sessionObject)
    {
        throw new System.NotImplementedException();
    }

    public Task<ITelegramCommandExecutionResult> Execute<TQuery>(ISessionTelegramCommand<TQuery, FluentObject<TObject>> currentCommand, TQuery query, FluentObject<TObject> sessionObject)
    {
        throw new System.NotImplementedException();
    }

    protected abstract Task Entry();
    protected abstract IStateMachine StateMachine(IStateMachineBuilder builder);
    protected abstract Task Finalize();
}

public interface IStateBuilder : IStateBuilderBase
{
    ICallbacksBuilder WithCallbacks();
}

public interface ICallbacksBuilderK
{
}

public interface IStateBuilderBase
{
    IStateMachineBuilder CommitState(Expression<Func<string, Task>> commitStateExpr);
}

public interface IStateMachineBuilder
{
    IStateBuilder Next(string message);
    IStateMachine Finish();
}

public interface IStateMachine
{
}

public class FluentObject<TObject>
{
    
}

internal class StateMachine : IStateMachine
{
    private Dictionary<int, State> _states;

    public StateMachine()
    {
        _states = new Dictionary<int, State>();
    }
}

internal class State
{
}

internal class StateMachineBuilder : IStateMachineBuilder
{
    private readonly StateMachine _stateMachine;

    public StateMachineBuilder()
    {
        _stateMachine = new StateMachine();
    }
    public IStateBuilder Next(string message)
    {
        
    }

    public IStateMachine Finish()
    {
        throw new System.NotImplementedException();
    }
}