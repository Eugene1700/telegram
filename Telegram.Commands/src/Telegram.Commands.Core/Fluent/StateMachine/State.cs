using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class State<TObj> : IState<TObj>
{
    private readonly StateType _stateType;

    public State(string id, StateType stateType, uint? durationInSec)
    {
        _stateType = stateType;
        Id = id;
        DurationInSec = durationInSec;
        _callbacksContainerRows = new List<CallbackDataContainerRow<TObj>>();
        _callbackIndex = new Dictionary<string, CallbackDataContainer<TObj>>();
    }

    public string Id { get; }
    public uint? DurationInSec { get; }

    public Task SendMessage<TQuery>(TQuery currentQuery, TObj obj)
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
        return _sendMessageProvider.Send(currentQuery, obj, mes);
    }

    public Task<string> HandleQuery<TQuery>(TQuery query, TObj obj)
    {
        if (CallbackDataContainer<TObj>.IsCallback(query))
        {
            return HandleCallback(query, obj);
        }
        return _handler(query, obj);
    }

    private Task<string> HandleCallback<TQuery>(TQuery query, TObj obj)
    {
        var (callbackKey, callbackUserData) = CallbackDataContainer<TObj>.ExtractData(query);
        if (_callbackIndex.TryGetValue(callbackKey, out var container))
        {
            return container.Commit(query, obj, callbackUserData);
        }

        throw new InvalidOperationException();
    }

    private Func<object, TObj, Task<string>> _handler;
    private readonly List<CallbackDataContainerRow<TObj>> _callbacksContainerRows;

    private Func<TObj, string> _message;
    private IMessageSender<TObj> _sendMessageProvider;
    private readonly Dictionary<string, CallbackDataContainer<TObj>> _callbackIndex;

    public void SetCommitter(Func<object, TObj, Task<string>> commitStateExpr)
    {
        _handler = commitStateExpr;
    }

    public bool IsCommandHandle(IQueryTelegramCommand<CallbackQuery> currentCommand)
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

    public void SetMessage(Func<TObj, string> messageProvider, IMessageSender<TObj> sendMessageProvider)
    {
        _message = messageProvider;
        _sendMessageProvider = sendMessageProvider;
    }
}