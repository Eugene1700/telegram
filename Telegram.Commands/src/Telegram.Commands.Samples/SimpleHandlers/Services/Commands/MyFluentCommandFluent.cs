using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;

namespace SimpleHandlers.Services.Commands;

public enum States
{
    Name,
    Surname,
    Validate,
    Exit
}

public enum FluentCallbacks
{
    DefaultName,
    Skip,
    Exit,
    Text,
    ReturnToEntry,
    DefaultSecondName,
    SendNumber
}

[Command(Name = "myfluent")]
public class MyFluentCommandFluent: FluentCommand<MyObject, States, FluentCallbacks>, IMessageSender<MyObject>
{
    private readonly ITelegramBotClient _telegramBotClient;

    public MyFluentCommandFluent(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    protected override Task<MyObject> Entry<TQuery>(TQuery query, MyObject myObject)
    {
        var chatId = query switch
        {
            Message message => message.GetChatId(),
            CallbackQuery callbackQuery => callbackQuery.GetChatId(),
            _ => throw new InvalidOperationException()
        };
        var o = myObject ?? new MyObject();
        o.ChatId = chatId;
        
        return Task.FromResult(o);
    }

    protected override IStateMachine<States> StateMachine(IStateMachineBuilder<MyObject, States, FluentCallbacks> builder)
    {
        var a = builder.Entry(States.Name).WithMessage(_ => Task.FromResult("Hi! What's your name?"), this)
            .WithCallbacks()
            .Row().OnCallback(FluentCallbacks.DefaultName, "Default Name (Jack)", "Jack", FirstNameCallbackHandler)
            .Row().NextFromCallback(FluentCallbacks.Skip, "Skip", "data", States.Surname)
            .Row().NextFromCallback(FluentCallbacks.Exit, "Exit", "data", States.Exit)
            .Row().OnCallback(FluentCallbacks.Text, "Send TEXT", "TEXT", SendUserDataHandler)
            .Row().ExitFromCallback<MyObject, States, FluentCallbacks, CancelCallback>("Cancel", "someData")
            .Row().ExitFromCallback<MyObject, States, FluentCallbacks, CancelCallback>("Cancel", "someData")
            .Row().ExitFromCallback(KeyBoardBuild,
                TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>())
            .ExitFromCallback(CallbackDataWithCommand())
            .KeyBoard(SameCallbackKey)
            .Next(FirstNameMessageHandler)
            .State(States.Surname)
            .WithMessage(obj => Task.FromResult($"Ok, send me your surname, {obj.FirstName}"), this)
            .WithCallbacks().KeyBoard(GetSurnameKeyboard)
            .Next(SecondNameHandler)
            .State(States.Validate).WithMessage("Your name is too short! Please, send me again", this)
            .WithCallbacks()
            .Row().NextFromCallback(FluentCallbacks.Skip, "Skip", "data", States.Surname)
            .Next(FirstNameMessageHandler)
            .Exit<object>(States.Exit, Finalize);
            
            return a.Build();
    }

    private Task SameCallbackKey(MyObject arg1, ICallbacksBuilderBase<MyObject, States, FluentCallbacks> arg2)
    {
        var b = arg2.Row();
        foreach (var num in Enumerable.Range(0, 5))
        {
            b.OnCallback(FluentCallbacks.SendNumber, $"{num}", $"{num}", SendUserDataHandler);
        }
        return Task.CompletedTask;
    }
    

    private Task GetSurnameKeyboard(MyObject arg1, ICallbacksBuilderBase<MyObject, States, FluentCallbacks> arg2)
    {
        arg2.Row().NextFromCallback(FluentCallbacks.Skip, "Skip", arg1.FirstName, States.Exit)
            .NextFromCallback(FluentCallbacks.ReturnToEntry, "Back", arg1.FirstName, States.Name)
            .OnCallback(FluentCallbacks.DefaultSecondName, "Default SecondName Smith", "Smith", SecondNameCallbackHandler)
            .Row().NextFromCallback(FluentCallbacks.Exit, "Finish", arg1.FirstName, States.Exit);
        return Task.CompletedTask;
    }

    private static CallbackDataWithCommand CallbackDataWithCommand()
    {
        return new CallbackDataWithCommand
        {
            Text = "Another another cancel",
            CallbackText = "data",
            CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>()
        };
    }

    private static CallbackData KeyBoardBuild(MyObject obj)
    {
        return new CallbackDataWithCommand
            {
                Text = "Another cancel",
                CallbackText = "data",
                CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>()
            };
    }

    private async Task<States> SendUserDataHandler(CallbackQuery arg1, MyObject arg2, string userData)
    {
        await _telegramBotClient.SendTextMessageAsync(arg2.ChatId, userData);
        return States.Name;
    }

    private static async Task<States> FirstNameCallbackHandler(CallbackQuery query, MyObject obj, string userData)
    {
        return await FirstNameHandler(userData, obj);
    }
    
    private async Task<States> SecondNameCallbackHandler(CallbackQuery query, MyObject obj, string userData)
    {
        return await SecondNameHandler(userData, obj);
    }
    
    private static async Task<States> FirstNameMessageHandler(Message query, MyObject obj)
    {
        return await FirstNameHandler(query.Text, obj);
    }

    private static async Task<States> FirstNameHandler(string text, MyObject obj)
    {
        if (text.Length < 2)
        {
            return States.Validate;
        }

        obj.FirstName = text;
        return States.Surname;
    }


    public async Task<States> SecondNameHandler(Message message, MyObject obj)
    {
        return await SecondNameHandler(message.Text, obj);
    }
    
    public async Task<States> SecondNameHandler(string text, MyObject obj)
    {
        obj.SecondName = text;
        return States.Exit;
    }

    private async Task<ITelegramCommandExecutionResult> Finalize(object currentQuery, MyObject obj)
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

        await _telegramBotClient.SendTextMessageAsync(chatId, $"Your data: {obj.FirstName} {obj.SecondName}");
        return TelegramCommandExecutionResult.Break();
    }

    public Task Send<TQuery>(TQuery currentQuery, MyObject obj, ITelegramMessage message)
    {
        return _telegramBotClient.SendTextMessageAsync(obj.ChatId, message.Message,
            replyMarkup: message.ReplyMarkup);
    }
}

public class MyObject
{
    public string SecondName { get; set; }
    public string FirstName { get; set; }

    public long ChatId { get; set; }
}