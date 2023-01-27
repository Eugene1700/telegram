using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Models;

public abstract class FluentCommand<TObject> : IBehaviorTelegramCommand<FluentObject<TObject>>
{
    public const string DefaultMoveCondition = "default_condition";
    public async Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query,
        FluentObject<TObject> sessionObject)
    {
        var builderStateMachine = new StateMachineBuilder<TObject>();
        var stateMachine = (StateMachine<TObject>) StateMachine(builderStateMachine);
        if (sessionObject.CurrentStateId == null)
        {
            await Entry();
            sessionObject.CurrentStateId = 0;
            var entryState = stateMachine.GetCurrentStateInternal(sessionObject.CurrentStateId.Value);
            var nextMessage = entryState.GetMessage();
            await SendMessage(query, nextMessage);
            return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, null);
        }
        var currentState = stateMachine.GetCurrentStateInternal(sessionObject.CurrentStateId.Value);
        var condition = query switch
        {
            Message message => await currentState.Commit(message.Text, sessionObject.Object),
            CallbackQuery callbackQuery => await currentState.CallbackCommit(callbackQuery.Data, sessionObject.Object),
        };

        var next = currentState.Next(condition);
        if (next == null)
        {
            return await Finalize(query, sessionObject.Object);
        }

        if (next.Id != currentState.Id)
        {
            var nextMessage = next.GetMessage();
            await SendMessage(query, nextMessage);
        }

        sessionObject.CurrentStateId = next.Id;
        return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, null);
    }

    protected abstract Task SendMessage<TQuery>(TQuery currentQuery, ITelegramMessage nextMessage);

    public Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand,
        TQuery query, FluentObject<TObject> sessionObject)
    {
        return DefaultExecute(query, sessionObject);
    }

    public Task<ITelegramCommandExecutionResult> Execute<TQuery>(
        ISessionTelegramCommand<TQuery, FluentObject<TObject>> currentCommand, TQuery query,
        FluentObject<TObject> sessionObject)
    {
        return DefaultExecute(query, sessionObject);
    }

    protected abstract Task<TObject> Entry();
    protected abstract IStateMachine<TObject> StateMachine(IStateMachineBuilder<TObject> builder);
    protected abstract Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery currentQuery, TObject obj);
}

public interface IStateBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> WithCallbacks();
}

public interface ICallbacksBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbacksBuilder<TObj> ExitState(string text, string data, Func<string, TObj, Task<string>> commitExpr, string condition, IStateBase<TObj> state);

    ICallbacksBuilder<TObj> MoveNextCommand<TCommand>(string text, string data)
        where TCommand : IQueryTelegramCommand<CallbackQuery>;

    ICallbacksBuilder<TObj> MoveNextCommand<TCommand, TSessionObject>(string text, string data)
        where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>;

    ICallbacksBuilder<TObj> CommitData(string text, string data, Func<string, TObj, Task<string>> commitExpr);
    ICallbacksBuilder<TObj> ExitState(string commitStateExpr, string data, IStateBase<TObj> nextState);
}

public interface IStateBuilderBase<TObj>
{
    IStateBase<TObj> GetCurrentState();
    IStateMoverBuilder<TObj> ExitState(Func<string, TObj, Task<string>> commitStateExpr);
    IStateMoverBuilder<TObj> ExitState(string condition);
}

public interface IStateMoverBuilder<TObj>
{
    IStateMachineBuilder<TObj> Next(string condition, IStateBase<TObj> nextState);
    IStateMachineBuilder<TObj> Next(IStateBase<TObj> nextState);
}

public interface IStateMachineBuilder<TObj>
{
    IStateBuilder<TObj> NewState(string message);
    IStateMachine<TObj> Finish();
}

public interface IStateMachine<TObj>
{
    IStateBase<TObj> GetCurrentState(int currentStateId);
}
public class FluentObject<TObject>
{
    public FluentObject(TObject o)
    {
        Object = o;
        CurrentStateId = null;
    }

    public int? CurrentStateId { get; set; }
    public TObject Object { get; set; }
}

internal class StateMachine<TObj> : IStateMachine<TObj>
{
    private Dictionary<int, IState<TObj>> _states;

    public StateMachine()
    {
        _states = new Dictionary<int, IState<TObj>>();
    }

    private int GetId()
    {
        return _states.Any() ? _states.Max(x => x.Key) + 1 : 0;
    }

    public State<TObj> AddState()
    {
        var id = GetId();
        var newState = new State<TObj>(id);
        _states.Add(id, newState);
        return newState;
    }

    public IStateBase<TObj> GetCurrentState(int currentStateId)
    {
        return GetCurrentStateInternal(currentStateId);
    }
    
    public IState<TObj> GetCurrentStateInternal(int currentStateId)
    {
        return _states[currentStateId];
    }
}

internal class State<TObj> : IState<TObj>
{
    public State(int id)
    {
        Id = id;
        _callbacksCommiterrs = new Dictionary<string, Func<string, TObj, Task<string>>>();
        _callbacks = new List<CallbackData>();
        _conditions = new Dictionary<string, IState<TObj>>();
    }

    public int Id { get; }

    public ITelegramMessage GetMessage()
    {
        IReplyMarkup replyMarkup = null;
        if (_callbacks.Any())
        {
            var builder = new InlineMarkupQueryBuilder();
            foreach (var callback in _callbacks)
            {
                if (callback is CallbackDataWithCommand callbackDataWithCommand)
                {
                    builder.AddInlineKeyboardButton(callbackDataWithCommand);
                }

                builder.AddInlineKeyboardButton(callback);
            }

            replyMarkup = builder.GetResult();
        }

        return new TelegramMessage(_message, replyMarkup);
    }

    public async Task<string> Commit(string message, TObj obj)
    {
        return await _committer(message, obj);
    }

    public async Task<string> CallbackCommit(string data, TObj obj)
    {
        if (_callbacksCommiterrs.TryGetValue(data, out var committer))
        {
            return await committer(data, obj);
        }

        throw new InvalidOperationException();
    }

    private Func<string, TObj, Task<string>> _committer;
    private readonly Dictionary<string, IState<TObj>> _conditions;

    private readonly List<CallbackData> _callbacks;

    private Dictionary<string, Func<string, TObj, Task<string>>> _callbacksCommiterrs;
    private string _message;

    public void SetMessage(string text)
    {
        _message = text;
    }

    public void SetCommitter(Func<string, TObj, Task<string>> commitStateExpr)
    {
        _committer = commitStateExpr;
    }

    public void AddCondition(string condition, IState<TObj> newState)
    {
        _conditions.Add(condition, newState);
    }

    public IState<TObj> Next(string condition)
    {
        return _conditions.TryGetValue(condition, out var nextState) ? nextState : null;
    }

    public void AddCallback(CallbackData callbackData, Func<string, TObj, Task<string>> commitExpr)
    {
        _callbacks.Add(callbackData);
        _callbacksCommiterrs[callbackData.CallbackText] = commitExpr;
    }

    public void AddCallback(CallbackDataWithCommand callbackData)
    {
        _callbacks.Add(callbackData);
    }
}

internal class TelegramMessage : ITelegramMessage
{
    public TelegramMessage(string message, IReplyMarkup replyMarkup)
    {
        Message = message;
        ReplyMarkup = replyMarkup;
    }

    public string Message { get; }

    public IReplyMarkup ReplyMarkup { get; }
}

internal interface IState<TObj> : IStateBase<TObj>
{
    Task<string> Commit(string message, TObj obj);
    Task<string> CallbackCommit(string data, TObj obj);
    IState<TObj> Next(string condition);
}

public interface IStateBase<T>
{
    int Id { get; }
    ITelegramMessage GetMessage();
}

public interface ITelegramMessage
{
    string Message { get; }
    IReplyMarkup ReplyMarkup { get; }
}

internal class StateMachineBuilder<TObj> : IStateMachineBuilder<TObj>
{
    private readonly StateMachine<TObj> _stateMachine;

    public StateMachineBuilder()
    {
        _stateMachine = new StateMachine<TObj>();
    }

    public IStateBuilder<TObj> NewState(string message)
    {
        var newState = _stateMachine.AddState();
        newState.SetMessage(message);
        var entryStateBuilder = new StateBuilder<TObj>(newState, this);
        return entryStateBuilder;
    }

    public IStateMachine<TObj> Finish()
    {
        return _stateMachine;
    }

    public State<TObj> GetNextState()
    {
        return _stateMachine.AddState();
    }
}

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

    public IStateMachineBuilder<TObj> Next(string condition, IStateBase<TObj> nextState)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException();
        }

        _state.AddCondition(condition, nextState as IState<TObj>);
        return _stateMachineBuilder;
    }

    public IStateMachineBuilder<TObj> Next(IStateBase<TObj> nextState)
    {
        return Next(FluentCommand<TObj>.DefaultMoveCondition, nextState);
    }

    public IStateBase<TObj> GetCurrentState()
    {
        return _state;
    }
    
    public ICallbacksBuilder<TObj> ExitState(string text, string data, Func<string, TObj, Task<string>> commitExpr, string condition, IStateBase<TObj> state)
    {
        _state.AddCallback(new CallbackData
        {
            Text = text,
            CallbackText = data
        }, commitExpr);
        _state.AddCondition(condition, state as IState<TObj>);
        return this;
    }

    public ICallbacksBuilder<TObj> MoveNextCommand<TCommand>(string text, string data)
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

    public ICallbacksBuilder<TObj> MoveNextCommand<TCommand, TSessionObject>(string text, string data)
        where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
    {
        _state.AddCallback(new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data,
            CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery, TSessionObject>()
        });
        return this;
    }

    public ICallbacksBuilder<TObj> CommitData(string text, string data, Func<string, TObj, Task<string>> commitExpr)
    {
        _state.AddCallback(new CallbackData
        {
            Text = text,
            CallbackText = data,
        }, commitExpr);
        return this;
    }

    public ICallbacksBuilder<TObj> ExitState(string text, string data, IStateBase<TObj> nextState)
    {
        var condition = $"newCondition_from_{_state.Id}_to_{nextState.Id}";
        Func<string,TObj,Task<string>> commitExpr = (d, o) => Task.FromResult(condition);
        _state.AddCallback(new CallbackData
        {
            Text = text,
            CallbackText = data
        }, commitExpr);
        _state.AddCondition(condition, nextState as IState<TObj>);
        return this;
    }
}