using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine;

internal class State<TObj, TStates, TCallbacks> : IState<TObj, TStates> where TCallbacks : struct, Enum
{
    private readonly StateType _stateType;
    public TStates Id { get; }
    public uint? DurationInSec { get; }
    internal CallbackBuilder<TObj, TStates, TCallbacks> CallbackBuilder { get; }

    public State(TStates id, StateType stateType, uint? durationInSec, Func<object, TObj, Task<ITelegramCommandExecutionResult>> finalizer = null)
    {
        _stateType = stateType;
        Id = id;
        DurationInSec = durationInSec;
        CallbackBuilder = new CallbackBuilder<TObj, TStates, TCallbacks>();
        _finalizer = finalizer;
    }

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

    public Task<TStates> HandleQuery<TQuery>(TQuery query, TObj obj)
    {
        if (CallbackDataContainer<TObj, TStates, TCallbacks>.IsCallback(query))
        {
            return HandleCallback(query, obj);
        }

        return _handler(query, obj);
    }

    private async Task<TStates> HandleCallback<TQuery>(TQuery query, TObj obj)
    {
        var callbacks = await CallbackBuilder.Build(obj);
        var (callbackKey, hash, callbackUserData) = CallbackDataContainer<TObj, TStates, TCallbacks>.ExtractData(query);
        CallbackDataContainer<TObj, TStates, TCallbacks> container = null;
        if (callbacks.Any(x => x.TryGetByKey(callbackKey, hash, obj, out container)))
        {
            return await container.Handle(query, obj, callbackUserData);
        }

        throw new InvalidOperationException();
    }

    private Func<object, TObj, Task<TStates>> _handler;

    private Func<TObj, Task<string>> _message;
    private IMessageSender<TObj> _sendMessageProvider;
    private readonly Func<object,TObj,Task<ITelegramCommandExecutionResult>> _finalizer;

    public void SetHandler(Func<object, TObj, Task<TStates>> handler)
    {
        _handler = handler;
    }

    public async Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand)
    {
        var curComDesc = currentCommand.GetCommandInfo();
        var callbacks = await CallbackBuilder.Build(obj);
        return callbacks.Any(x => x.HasCommand(curComDesc));
    }

    public StateType GetStateType()
    {
        return _stateType;
    }

    public Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery query, TObj sessionObjectObject)
    {
        return _finalizer?.Invoke(query, sessionObjectObject);
    }

    public void SetMessage(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sendMessageProvider)
    {
        _message = messageProvider;
        _sendMessageProvider = sendMessageProvider;
    }
}