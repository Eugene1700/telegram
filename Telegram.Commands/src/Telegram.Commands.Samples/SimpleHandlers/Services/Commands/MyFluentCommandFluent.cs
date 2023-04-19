﻿using System;
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
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders.Extensions;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;

namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent")]
public class MyFluentCommandFluent: FluentCommand<MyObject>, IMessageSender<MyObject>
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

    public enum States
    {
        Name,
        Surname,
        Validate,
        Exit
    }

    protected override IStateMachine<MyObject> StateMachine(IStateMachineBuilder<MyObject> builder)
    {
        return builder.Entry(States.Name).WithMessage(_ => Task.FromResult("Hi! What's your name?"), this)
            .WithCallbacks()
            .Row().OnCallback("defaultName", "Default Name (Jack)", "Jack", FirstNameCallbackHandler)
            .Row().NextFromCallback("skip", "Skip", "data", States.Surname)
            .Row().NextFromCallback("exit", "Exit", "data", States.Exit)
            .Row().OnCallback("sendText", "Send TEXT", "TEXT", SendTextHandler)
            .Row().ExitFromCallback<MyObject, CancelCallback>("Cancel", "someData")
            .Row().ExitFromCallback<MyObject, CancelCallback>("Cancel", "someData")
            .Row().ExitFromCallback(KeyBoardBuild,
                TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>())
            .ExitFromCallback(CallbackDataWithCommand())
            .Next(FirstNameMessageHandler)
            .State(States.Surname)
            .WithMessage(obj => Task.FromResult($"Ok, send me your surname, {obj.FirstName}"), this)
            .WithCallbacks().KeyBoard(GetSurnameKeyboard)
            .Next(SecondNameHandler)
            .State(States.Validate).WithMessage("Your name is too short! Please, send me again", this)
            .WithCallbacks()
            .Row().NextFromCallback("skip", "Skip", "data", States.Surname)
            .Next(FirstNameMessageHandler)
            .Exit<MyObject, States, object>(States.Exit, Finalize).Build();
    }

    private Task GetSurnameKeyboard(MyObject arg1, ICallbacksBuilderBase<MyObject> arg2)
    {
        arg2.Row().NextFromCallback("skip", "Skip", arg1.FirstName, States.Exit)
            .NextFromCallback("returnToEntry", "Back", arg1.FirstName, States.Name)
            .OnCallback("defaultSecondName", "Default SecondName Smith", "Smith", SecondNameCallbackHandler)
            .Row().NextFromCallback("def", "Finish", arg1.FirstName, States.Exit);
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

    private async Task<States> SendTextHandler(CallbackQuery arg1, MyObject arg2, string userData)
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