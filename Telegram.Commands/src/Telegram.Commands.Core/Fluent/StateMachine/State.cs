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
    private readonly StateType _stateType;

    public State(string id, StateType stateType)
    {
        _stateType = stateType;
        Id = id;
        _callbacksContainerRows = new List<CallbackDataContainerRow<TObj>>();
        _callbackIndex = new Dictionary<string, CallbackDataContainer<TObj>>();
    }

    public string Id { get; }

    public Task SendMessage(TObj obj)
    {
        IReplyMarkup replyMarkup = null;
        if (_callbacksContainerRows.Any())
        {
            var builder = new InlineMarkupQueryBuilder();
            foreach (var callbacksContainer in _callbacksContainerRows)
            {
                var callbacksRow = callbacksContainer.GetContainers();
                var row = callbacksRow.Select(x => x.Build(obj)).ToArray();
                builder.InlineKeyboardButtonsRow(row);
            }

            replyMarkup = builder.GetResult();
        }

        var messageText = _message(obj);
        var mes = new TelegramMessage(messageText, replyMarkup);
        return _sendMessageProvider(obj, mes);
    }

    public Task<string> Commit<TQuery>(TQuery query, TObj obj)
    {
        return _committer(query, obj);
    }

    public Task<string> CallbackCommit<TQuery>(TQuery query, TObj obj)
    {
        var data = query.GetData().Split(" ").FirstOrDefault();
        if (_callbackIndex.TryGetValue(data, out var container))
        {
            return container.Commit(query, obj);
        }

        throw new InvalidOperationException();
    }

    private Func<object, TObj, Task<string>> _committer;
    private readonly List<CallbackDataContainerRow<TObj>> _callbacksContainerRows;

    private Func<TObj, string> _message;
    private Func<TObj,ITelegramMessage,Task> _sendMessageProvider;
    private Dictionary<string, CallbackDataContainer<TObj>> _callbackIndex;

    public void SetMessage(Func<TObj, string> messageProvider, Func<TObj, ITelegramMessage, Task> sendMessageProvider)
    {
        _message = messageProvider;
        _sendMessageProvider = sendMessageProvider;
    }

    public void SetCommitter(Func<object, TObj, Task<string>> commitStateExpr)
    {
        _committer = commitStateExpr;
    }

    public bool CanNext(IQueryTelegramCommand<CallbackQuery> currentCommand)
    {
        var curComDesc = currentCommand.GetCommandInfo();
        return _callbacksContainerRows.Any(x => x.HasCommand(curComDesc));
    }

    public StateType GetStateType()
    {
        return _stateType;
    }


    public CallbackDataContainerRow<TObj> AddCallbackRow()
    {
        var newRow = new CallbackDataContainerRow<TObj>();
        _callbacksContainerRows.Add(newRow);
        return newRow;
    }

    public void AddIndex(string callbackId, CallbackDataContainer<TObj> container)
    {
        _callbackIndex.Add(callbackId, container);
    }
}