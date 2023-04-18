using System;
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
        CallbackBuilder = new CallbackBuilder<TObj>();
    }

    public string Id { get; }
    public uint? DurationInSec { get; }
    internal CallbackBuilder<TObj> CallbackBuilder { get; }

    public async Task SendMessage<TQuery>(TQuery currentQuery, TObj obj)
    {
        IReplyMarkup replyMarkup = null;
        var callbacks = await CallbackBuilder?.Build(obj);
        if (callbacks != null && callbacks.Any())
        {
            var builder = new InlineMarkupQueryBuilder();
            foreach (var callbacksContainerRowsProvider in callbacks)
            {
                var callbacksRow = callbacksContainerRowsProvider.GetContainers();
                var row = callbacksRow.Select(x => x.Build(obj)).ToArray();
                builder.InlineKeyboardButtonsRow(row);
            }

            replyMarkup = builder.GetResult();
        }

        var messageText = await _message(obj);
        var mes = new TelegramMessage(messageText, replyMarkup);
        await _sendMessageProvider.Send(currentQuery, obj, mes);
    }

    public Task<string> HandleQuery<TQuery>(TQuery query, TObj obj)
    {
        if (CallbackDataContainer<TObj>.IsCallback(query))
        {
            return HandleCallback(query, obj);
        }

        return _handler(query, obj);
    }

    private async Task<string> HandleCallback<TQuery>(TQuery query, TObj obj)
    {
        var callbacks = await CallbackBuilder.Build(obj);
        var (callbackKey, callbackUserData) = CallbackDataContainer<TObj>.ExtractData(query);
        CallbackDataContainer<TObj> container = null;
        if (callbacks.Any(x => x.TryGetByKey(callbackKey, out container)))
        {
            return await container.Commit(query, obj, callbackUserData);
        }

        throw new InvalidOperationException();
    }

    private Func<object, TObj, Task<string>> _handler;

    private Func<TObj, Task<string>> _message;
    private IMessageSender<TObj> _sendMessageProvider;

    public void SetCommitter(Func<object, TObj, Task<string>> commitStateExpr)
    {
        _handler = commitStateExpr;
    }

    public async Task<bool> IsCommandHandle(TObj obj, IQueryTelegramCommand<CallbackQuery> currentCommand)
    {
        var curComDesc = currentCommand.GetCommandInfo();
        var callbacks = await CallbackBuilder.Build(obj);
        return callbacks.Any(x => x.HasCommand(curComDesc));
    }

    public StateType GetStateType()
    {
        return _stateType;
    }

    public void SetMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sendMessageProvider)
    {
        _message = messageProvider;
        _sendMessageProvider = sendMessageProvider;
    }
}