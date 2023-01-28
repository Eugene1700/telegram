using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class State<TObj> : IState<TObj>
{
    public State(int id)
    {
        Id = id;
        _callbacksCommitters = new Dictionary<string, Func<string, TObj, Task<string>>>();
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
                    continue;
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
        if (_callbacksCommitters.TryGetValue(data, out var committer))
        {
            return await committer(data, obj);
        }

        throw new InvalidOperationException();
    }

    private Func<string, TObj, Task<string>> _committer;
    private readonly Dictionary<string, IState<TObj>> _conditions;

    private readonly List<CallbackData> _callbacks;

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
        var moveToAnotherCommand = _callbacks.Select(x => x as CallbackDataWithCommand).Where(x => x != null).ToArray();
        return moveToAnotherCommand.Any(x => x.CommandDescriptor.MatchCommand(curComDesc));
    }

    public void AddCallback(CallbackData callbackData, Func<string, TObj, Task<string>> commitExpr)
    {
        _callbacks.Add(callbackData);
        _callbacksCommitters[callbackData.CallbackText] = commitExpr;
    }

    public void AddCallback(CallbackDataWithCommand callbackData)
    {
        _callbacks.Add(callbackData);
    }
}