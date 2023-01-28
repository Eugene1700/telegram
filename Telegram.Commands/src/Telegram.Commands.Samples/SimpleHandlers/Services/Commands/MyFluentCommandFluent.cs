using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Fluent.Builders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;

namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent")]
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

    protected override Task<MyObject> Entry<TQuery>(TQuery query)
    {
        var chatId = query switch
        {
            Message message => message.GetChatId(),
            CallbackQuery callbackQuery => callbackQuery.GetChatId(),
            _ => throw new InvalidOperationException()
        };
        return Task.FromResult(new MyObject
        {
            ChatId = chatId
        });
    }

    protected override IStateMachine<MyObject> StateMachine(IStateMachineBuilder<MyObject> builder)
    {
        var nameStateBuilder = builder.NewState("Hi! What's your name?");

        var secondNameStateBuilder = builder.NewState("Ok, send me your surname");
        var secondNameState = secondNameStateBuilder.GetState();
        secondNameStateBuilder.ExitState(GetSecondNameCommitter);

        var validateNameBuilder = builder.NewState("Your name is too short! Please, send me again");
        var validateState = validateNameBuilder.GetState();
        validateNameBuilder
            .WithCallbacks()
            .ExitStateByCallback("Skip", "data", secondNameState)
            .ExitState(FirstNameCommitter)
            .Loop("toolittleName")
            .Next(secondNameState);

        nameStateBuilder.WithCallbacks()
            .ExitStateByCallback("Default Name (Jack)", "Jack", FirstNameCommitter)
            .ExitStateByCallback("Skip", "data", secondNameState)
            .ExitStateByCallback<CancelCallback>("Cancel", "somedata")
            .ExitStateByCallback("Send TEXT", "TEXT", SendTextCommitter)
            .ExitStateByCallback(()=> new IEnumerable<CallbackDataWithCommand>[] {new []{ new CallbackDataWithCommand
            {
                Text = "Another cancel",
                CallbackText = "data",
                CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>()
            },
                new CallbackDataWithCommand
                {
                    Text = "Another another cancel",
                    CallbackText = "data",
                    CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>()
                }
            }})
            .ExitState(FirstNameCommitter)
            .Next(secondNameState)
            .Loop("sendtextcondition")
            .ConditionNext("toolittleName", validateState);

        return builder.Finish();
    }

    private async Task<string> SendTextCommitter(string arg1, MyObject arg2)
    {
        await _telegramBotClient.SendTextMessageAsync(arg2.ChatId, arg1);
        return "sendtextcondition";
    }

    private async Task<string> FirstNameCommitter(string message, MyObject obj)
    {
        if (message.Length < 2)
        {
            return "toolittleName";
        }

        obj.FirstName = message;
        return DefaultNextCondition;
    }

    public async Task<string> GetSecondNameCommitter(string message, MyObject obj)
    {
        obj.SecondName = message;
        return DefaultNextCondition;
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

        await _telegramBotClient.SendTextMessageAsync(chatId, $"Your data: {obj.FirstName} {obj.SecondName}");
        return TelegramCommandExecutionResult.Break();
    }
}

public class MyObject
{
    public string SecondName { get; set; }
    public string FirstName { get; set; }
    
    public long ChatId { get; set; }
}