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

    public State(string id, StateType stateType, uint? durationInSec, ICallbacksBuilder<TObj> callbacksBuilder)
    {
        _stateType = stateType;
        Id = id;
        DurationInSec = durationInSec;
        _callbackBuilder = new CallbackBuilder<TObj>(callbacksBuilder);
    }

    public string Id { get; }
    public uint? DurationInSec { get; }

    public async Task SendMessage<TQuery>(TQuery currentQuery, TObj obj)
    {
        IReplyMarkup replyMarkup = null;
        var callbacks = await _callbackBuilder.Build(obj);
        if (callbacks.Any())
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

        var messageText = _message(obj);
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
        var callbacks =  await _callbackBuilder.Build(obj);
        var (callbackKey, callbackUserData) = CallbackDataContainer<TObj>.ExtractData(query);
        callbacks.Any(x=>x.HasCommand())
        if (_callbackIndex.TryGetValue(callbackKey, out var container))
        {
            return container.Commit(query, obj, callbackUserData);
        }

        throw new InvalidOperationException();
    }

    private Func<object, TObj, Task<string>> _handler;
    private readonly CallbackBuilder<TObj> _callbackBuilder;

    private Func<TObj, string> _message;
    private IMessageSender<TObj> _sendMessageProvider;

    public void SetCommitter(Func<object, TObj, Task<string>> commitStateExpr)
    {
        _handler = commitStateExpr;
    }

    public bool IsCommandHandle(IQueryTelegramCommand<CallbackQuery> currentCommand)
    {
        var curComDesc = currentCommand.GetCommandInfo();
        return _callbacksContainerRowsProviders.Any(x => x().Any(y => y.HasCommand(curComDesc)));
    }

    public StateType GetStateType()
    {
        return _stateType;
    }


    public CallbackDataContainerRow<TObj> AddCallbackRow()
    {
        var newRow = new CallbackDataContainerRow<TObj>();
        _callbacksContainerRowsProviders.Add(() => new[] { newRow });
        return newRow;
    }

    public void SetMessage(Func<TObj, string> messageProvider, IMessageSender<TObj> sendMessageProvider)
    {
        _message = messageProvider;
        _sendMessageProvider = sendMessageProvider;
    }

    public void AddCallbackKeyboard(Func<TObj, Task> provider)
    {
        _callbacksContainerRowsProviders.Add();
    }
}

internal class CallbackBuilder<TObj>
{
    private readonly ICallbacksBuilder<TObj> _builder;
    private readonly List<Func<TObj, ICallbacksBuilder<TObj>, Task>> _providers;


    public CallbackBuilder(ICallbacksBuilder<TObj> builder)
    {
        _builder = builder;
    }
    
    public void AddProvider(Func<TObj, ICallbacksBuilder<TObj>, Task> provider)
    {
        _providers.Add(provider);
    }
    
    public Task<CallbackDataContainerRow<TObj>[]> Build(TObj obj)
    {
        var res = new List<CallbackDataContainerRow<TObj>>();
        foreach (var provider in _providers)
        {
            provider(obj, _builder).GetAwaiter();
        }
        
        return _builder   
    }
}