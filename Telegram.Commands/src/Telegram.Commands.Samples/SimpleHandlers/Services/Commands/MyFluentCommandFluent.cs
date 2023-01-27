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

[Command(Name = "my_fluent")]
public class MyFluentCommandFluent : FluentCommand<MyObject>
{
    private readonly ITelegramBotClient _telegramBotClient;

    public MyFluentCommandFluent(ITelegramBotClient telegramBotClient)
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
        var nameStateBuilder = builder.NewState().Message("Привет! Введи свое имя");
        var stateMoverBuilder = nameStateBuilder.ExitState(FirstNameCommitter);
        
        var secondNameStateBuilder = builder.NewState().Message("Введи свою фамилию");
        var secondNameMover = secondNameStateBuilder.ExitState(GetSecondNameCommitter);
        
        stateMoverBuilder.Next("", secondNameMover.GetCurrentState());
        
        var validateSecondNameBuilder = builder.NewState().Message("Слишком короткое имя! Введите еще раз");
        var validateSecondNameMover = validateSecondNameBuilder.ExitState(FirstNameCommitter);
        validateSecondNameMover.Next("toolittleName", validateSecondNameMover.GetCurrentState());
        validateSecondNameMover.Next("", secondNameMover.GetCurrentState());
        
        var callbacks2 = validateSecondNameBuilder.WithCallbacks();
        callbacks2.MoveToState("Пропустить", "data", (d, obj) => Task.FromResult("condition"), "condition",
            secondNameMover.GetCurrentState());
        
        
        stateMoverBuilder.Next("toolittleName", validateSecondNameMover.GetCurrentState());
        
        var callbacks = nameStateBuilder.WithCallbacks();
        callbacks.MoveToState("Пропустить", "data", (d, obj) => Task.FromResult("condition"), "condition",
            secondNameMover.GetCurrentState());
        return builder.Finish();
    }

    private async Task<string> FirstNameCommitter(string message, MyObject obj)
    {
        if (message.Length < 2)
        {
            return "toolittleName";
        }
        obj.FirstName = message;
        return "";
    }

    public async Task<string> GetSecondNameCommitter(string message, MyObject obj)
    {
        obj.SecondName = message;
        return "";
    }

    protected override async Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery currentQuery, MyObject obj)
    {
        long chatId = 0;
        if (currentQuery is Message message)
        {
            chatId = message.GetChatId();
        }
        
        if (currentQuery is CallbackQuery callbackQuery)
        {
            chatId = callbackQuery.GetChatId();
        }

        await _telegramBotClient.SendTextMessageAsync(chatId, $"Вот твои данные: {obj.FirstName} {obj.SecondName}");
        return TelegramCommandExecutionResult.Break();
    }
}

public class MyObject
{
    public string SecondName { get; set; }
    public string FirstName { get; set; }
}