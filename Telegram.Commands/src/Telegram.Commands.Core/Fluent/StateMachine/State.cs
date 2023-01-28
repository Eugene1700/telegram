using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class State<TObj> : IState<TObj>
{
    public State(int id)
    {
        Id = id;
        _callbacksCommitters = new Dictionary<string, Func<string, TObj, Task<string>>>();
        _callbacksContainers = new List<CallbackDataContainer>();
        _conditions = new Dictionary<string, IState<TObj>>();
    }

    public int Id { get; }

    public ITelegramMessage GetMessage()
    {
        IReplyMarkup replyMarkup = null;
        if (_callbacksContainers.Any())
        {
            var builder = new InlineMarkupQueryBuilder();
            foreach (var callbacksContainer in _callbacksContainers)
            {
                var callbacksRows = callbacksContainer.Build();
                foreach (var callbackRow in callbacksRows)
                {
                    builder.InlineKeyboardButtonsRow(callbackRow.ToArray());
                }
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
        if (_callbacksCommitters.TryGetValue(data, out var committer))
        {
            return await committer(data, obj);
        }

        throw new InvalidOperationException();
    }

    private Func<string, TObj, Task<string>> _committer;
    private readonly Dictionary<string, IState<TObj>> _conditions;

    private readonly List<CallbackDataContainer> _callbacksContainers;

    private readonly Dictionary<string, Func<string, TObj, Task<string>>> _callbacksCommitters;
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

    public bool CanNext(IQueryTelegramCommand<CallbackQuery> currentCommand)
    {
        var curComDesc = currentCommand.GetCommandInfo();
        return _callbacksContainers.Any(x => x.HasCommand(curComDesc));
    }

    public void AddCallback(CallbackDataWithCommand callbackData, Func<string, TObj, Task<string>> commitExpr)
    {
        _callbacksContainers.Add(new CallbackDataContainer(callbackData));
        _callbacksCommitters[callbackData.CallbackText] = commitExpr;
    }

    public void AddCallback(CallbackDataWithCommand callbackData)
    {
        _callbacksContainers.Add(new CallbackDataContainer(callbackData));
    }
    
    public void AddCallback(Func<IEnumerable<IEnumerable<CallbackDataWithCommand>>> builder)
    {
        _callbacksContainers.Add(new CallbackDataContainer(builder));
    }
}

internal class CallbackDataContainer
{
    private readonly Func<IEnumerable<IEnumerable<CallbackDataWithCommand>>> _builder;
    private readonly CallbackDataWithCommand[] _callbackData;

    public CallbackDataContainer(CallbackDataWithCommand callbackData)
    {
        _callbackData = new[] { callbackData };
    }

    public CallbackDataContainer(CallbackDataWithCommand[] callbackData)
    {
        _callbackData = callbackData;
    }

    public CallbackDataContainer(Func<IEnumerable<IEnumerable<CallbackDataWithCommand>>> builder)
    {
        _builder = builder;
    }

    public IEnumerable<IEnumerable<CallbackDataWithCommand>> Build()
    {
        if (_callbackData != null && _callbackData.Any())
            return new[] { _callbackData };

        if (_builder != null)
            return _builder();

        throw new InvalidOperationException();
    }

    public bool HasCommand(ITelegramCommandDescriptor descriptor)
    {
        if (_callbackData.Any(x => x.CommandDescriptor != null && x.CommandDescriptor.MatchCommand(descriptor)))
            return true;

        if (_builder != null)
        {
            var callbacks = _builder();
            return callbacks.Select(callback => callback.Any(x => x.CommandDescriptor != null && x.CommandDescriptor.MatchCommand(descriptor))).FirstOrDefault();
        }

        return false;
    }
}