using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent")]
public class MyFluentCommand : FluentCommand<MyObject>
{
    private readonly ITelegramBotClient _telegramBotClient;

    public MyFluentCommand(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }
    protected override async Task SendMessage<TQuery>(TQuery currentQuery, ITelegramMessage nextMessage)
    {
        if (currentQuery is Message message)
        {
            await _telegramBotClient.SendTextMessageAsync(message.GetChatId(), nextMessage.Message,
                replyMarkup: nextMessage.ReplyMarkup);
        }
        
        if (currentQuery is CallbackQuery callbackQuery)
        {
            await _telegramBotClient.SendTextMessageAsync(callbackQuery.GetChatId(), nextMessage.Message,
                replyMarkup: nextMessage.ReplyMarkup);
        }
    }

    protected override Task<MyObject> Entry()
    {
        return Task.FromResult(new MyObject());
    }

    protected override IStateMachine<MyObject> StateMachine(IStateMachineBuilder<MyObject> builder)
    {
        var nameState = builder.Entry().NewMessage("Привет! Введи свое имя");
        var secondNameStateBuilder = nameState.CommitState((i, obj) => Task.FromResult(""))
            .Next().NewMessage("Введи свою фамилию");
        var secondNameState = secondNameStateBuilder.CommitState(GetSecondNameCommitter)
            .GetCurrentState();
        var callbacks = nameState.WithCallbacks();
        callbacks.MoveToState("Пропустить", "", (d, obj) => Task.FromResult(""), secondNameState);
        return builder.Finish();
    }

    public async Task<string> GetSecondNameCommitter(string message, MyObject obj)
    {
        obj.SecondName = message;
        return "";
    }

    protected override async Task<ITelegramCommandExecutionResult> Finalize(MyObject obj)
    {
        return TelegramCommandExecutionResult.Break();
    }
}

public class MyObject
{
    public string SecondName { get; set; }
}