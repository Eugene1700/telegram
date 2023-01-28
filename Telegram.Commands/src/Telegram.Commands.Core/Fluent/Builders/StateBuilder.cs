using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

internal class StateBuilder<TObj> : IStateBuilder<TObj>, IStateMoverBuilder<TObj>, ICallbacksBuilder<TObj>
{
    private readonly State<TObj> _state;
    private readonly StateMachineBuilder<TObj> _stateMachineBuilder;

    public StateBuilder(State<TObj> state, StateMachineBuilder<TObj> stateMachineBuilder)
    {
        _state = state;
        _stateMachineBuilder = stateMachineBuilder;
    }

    public IStateMoverBuilder<TObj> ExitState(Func<string, TObj, Task<string>> commitStateExpr)
    {
        _state.SetCommitter(commitStateExpr);
        return this;
    }

    public IStateMoverBuilder<TObj> ExitState(string condition)
    {
        Task<string> Cond(string s, TObj obj) => Task.FromResult(condition);
        return ExitState(Cond);
    }

    public ICallbacksBuilder<TObj> WithCallbacks()
    {
        return this;
    }

    public IStateMoverBuilder<TObj> ConditionNext(string condition, IStateBase<TObj> nextState)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException();
        }

        _state.AddCondition(condition, nextState as IState<TObj>);
        return this;
    }

    public IStateMoverBuilder<TObj> Loop(string condition)
    {
        return ConditionNext(condition, _state);
    }

    public IStateMoverBuilder<TObj> Next(IStateBase<TObj> nextState)
    {
        return ConditionNext(FluentCommand<TObj>.DefaultNextCondition, nextState);
    }

    public IStateMachineBuilder<TObj> Finish()
    {
        return _stateMachineBuilder;
    }

    public IStateBase<TObj> GetState()
    {
        return _state;
    }

    public ICallbacksBuilder<TObj> ExitStateByCallback(string text, string data, Func<string, TObj, Task<string>> commitExpr)
    {
        _state.AddCallback(new CallbackData
        {
            Text = text,
            CallbackText = data
        }, commitExpr);
        return this;
    }

    public ICallbacksBuilder<TObj> ExitStateByCallback<TCommand>(string text, string data)
        where TCommand : IQueryTelegramCommand<CallbackQuery>
    {
        _state.AddCallback(new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data,
            CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>()
        });
        return this;
    }
    
    public ICallbacksBuilder<TObj> ExitStateByCallback(string text, string data, IStateBase<TObj> nextState)
    {
        var condition = $"new_condition_from_{_state.Id}_to_{nextState.Id}";
        Task<string> CommitExpr(string s, TObj obj) => Task.FromResult(condition);
        _state.AddCallback(new CallbackData
        {
            Text = text,
            CallbackText = data
        }, CommitExpr);
        _state.AddCondition(condition, nextState as IState<TObj>);
        return this;
    }
}