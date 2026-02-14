using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Messages;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;
using Telegram.Commands.UI.Pagination;

namespace SimpleHandlers.Services.Commands
{
    public enum States
    {
        Name,
        Surname,
        Validate,
        Exit,
        Break
    }

    [Command(Name = "myfluent")]
    public class MyFluentCommandFluent : FluentCommand<MyObject, States>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public MyFluentCommandFluent(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        protected override Task<(MyObject, States)> Entry<TQuery>(TQuery query, MyObject myObject)
        {
            var chatId = query switch
            {
                Message message => message.GetChatId(),
                CallbackQuery callbackQuery => callbackQuery.GetChatId(),
                _ => throw new InvalidOperationException()
            };
            var o = myObject ?? new MyObject();
            o.ChatId = chatId;

            return Task.FromResult((o, States.Name));
        }

        protected override IStateMachine<States> StateMachine(
            IStateMachineBuilder<MyObject, States> builder)
        {
            var a = builder.State(States.Name)
                .WithMessage((s,o) => Task.FromResult("Hi! What's your name?"), Send)
                .WithCallbacks()
                .Row().OnCallback("Default Name (Jack)", "Jack", FirstNameCallbackHandler,
                    true)
                .Row().NextFromCallback("Skip", "data", States.Surname, true)
                .Row().NextFromCallback( "Exit", "data", States.Exit, true)
                .Row().OnCallback("Send TEXT", "TEXT", SendUserDataHandler, false)
                .Row().NextFromCallback("Message with break", "", States.Break, false)
                .Row().ExitFromCallback<MyObject, States, CancelCallback>("Cancel", "someData")
                .Row().ExitFromCallback<MyObject, States, CancelCallback>("Cancel", "someData")
                .Row().ExitFromCallback(KeyBoardBuild,
                    TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>())
                .ExitFromCallback(CallbackDataWithCommand())
                .KeyBoard(SameCallbackKey)
                .KeyboardWithPagination(Paginator)
                .WithMessages(OtherMessages)
                // .WithMessage(GetMessageWithParseMode, SendWithParseMode)
                .WithMessage(GetMessageWithParseMode2, SendWithParseMode)
                .Next(FirstNameMessageHandler, true)
                .State(States.Surname)
                .WithMessage((state, obj) => Task.FromResult($"Ok, send me your surname, {obj.FirstName}"), Send)
                .WithCallbacks().KeyBoard(GetSurnameKeyboard)
                .Next(SecondNameHandler, true)
                .State(States.Validate)
                .WithMessage("Your name is too short! Please, send me again", Send)
                .WithCallbacks()
                .Row().NextFromCallback("Skip", "data", States.Surname, true)
                .Next(FirstNameMessageHandler, true)
                .State(States.Break)
                .WithMessage("Message without waiting answer", Send)
                .FireAndForget()
                .Exit<object>(States.Exit, Finalize);

            return a.Build();
        }
        private Task SendWithParseMode(object currentQuery, States arg2, MyObject obj, ITelegramMessageTyped<TelegramParseMode> message)
        {
            if (currentQuery is CallbackQuery callbackQuery)
            {
                return _telegramBotClient.EditMessageTextAsync(obj.ChatId, callbackQuery.Message.MessageId, message.Text,
                    message.ParseMode.ToParseMode(),
                    replyMarkup: (InlineKeyboardMarkup)message.ReplyMarkup);
            }

            return _telegramBotClient.SendTextMessageAsync(obj.ChatId, message.Text,
                message.ParseMode.ToParseMode(),
                replyMarkup: message.ReplyMarkup);
        }

        private Task<ITextMessageTyped<TelegramParseMode>> GetMessageWithParseMode(States arg1, MyObject arg2)
        {
            throw new NotImplementedException();
        }
        
        private Task<(string, TelegramParseMode)> GetMessageWithParseMode2(States arg1, MyObject arg2)
        {
            return Task.FromResult(("*MarkDownText*", TelegramParseMode.MarkDownV2));
        }

        private Task BackHandler<TQuery>(TQuery arg1, MyObject arg2, string arg3) where TQuery : class
        {
            return Task.CompletedTask;
        }

        private Task OtherMessages(States state, MyObject arg1, IStateBuilderBase<MyObject, States> builder)
        {
            builder.WithMessage("Next message", SendSubMessage)
                .WithCallbacks().Row().NextFromCallback(GetCallback, States.Name, true)
                .NextFromCallback(GetCallback2, States.Surname, true);
            return Task.CompletedTask;
        }

        private CallbackData GetCallback(States state, MyObject arg)
        {
            return new CallbackData
            {
                Text = "Some callbackData",
                CallbackText = "text"
            };
        }
        
        private CallbackData GetCallback2(States states, MyObject arg)
        {
            return new CallbackData
            {
                Text = "Some callbackData2",
                CallbackText = "text"
            };
        }

        private Task SendSubMessage(object currentQuery, States state, MyObject obj, ITelegramMessage message)
        {
            return _telegramBotClient.SendTextMessageAsync(obj.ChatId, message.Text,
                replyMarkup: message.ReplyMarkup);
        }

        private Task<IFluentPaginationMenu> Paginator(MyObject arg1, uint arg2,
            ICallbacksBuilderBase<MyObject, States> arg3)
        {
            var b = arg3.Row();
            arg1.Limit = 5;
            arg1.TotalCount = 100;
            var from = (int)(5 * (arg2 - 1));
            foreach (var num in Enumerable.Range(from, 5))
            {
                b.OnCallback($"{num}", $"{num}", SendUserDataHandler, false);
            }

            return Task.FromResult<IFluentPaginationMenu>(arg1);
        }

        private Task SameCallbackKey(States state, MyObject arg1, ICallbacksBuilderBase<MyObject, States> arg2)
        {
            var b = arg2.Row();
            foreach (var num in Enumerable.Range(0, 5))
            {
                b.OnCallback($"{num}", $"{num}", SendUserDataHandler, false);
            }

            return Task.CompletedTask;
        }


        private Task GetSurnameKeyboard(States state, MyObject arg1, ICallbacksBuilderBase<MyObject, States> arg2)
        {
            arg2.Row().NextFromCallback($"Skip for {state}", arg1.FirstName, States.Exit, true)
                .NextFromCallback("Back", arg1.FirstName, States.Name, true)
                .OnCallback("Default SecondName Smith", "Smith",
                    SecondNameCallbackHandler, true)
                .Row().NextFromCallback("Finish", arg1.FirstName, States.Exit, true);
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

        private static CallbackData KeyBoardBuild(States state, MyObject obj)
        {
            return new CallbackDataWithCommand
            {
                Text = "Another cancel",
                CallbackText = "data",
                CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>()
            };
        }

        private async Task<States> SendUserDataHandler(CallbackQuery arg1, States states, MyObject arg2, string userData)
        {
            await _telegramBotClient.SendTextMessageAsync(arg2.ChatId, userData);
            return States.Name;
        }

        private static async Task<States> FirstNameCallbackHandler(CallbackQuery query, States state, MyObject obj, string userData)
        {
            return await FirstNameHandler(userData, obj);
        }

        private async Task<States> SecondNameCallbackHandler(CallbackQuery query, States state, MyObject obj, string userData)
        {
            return await SecondNameHandler(userData, obj);
        }

        private static async Task<States> FirstNameMessageHandler(Message query, States state, MyObject obj)
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


        public async Task<States> SecondNameHandler(Message message, States states, MyObject obj)
        {
            return await SecondNameHandler(message.Text, obj);
        }

        public async Task<States> SecondNameHandler(string text, MyObject obj)
        {
            obj.SecondName = text;
            return States.Exit;
        }

        private async Task<ITelegramCommandExecutionResult> Finalize(object currentQuery, States  state, MyObject obj)
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

        public Task Send<TQuery>(TQuery currentQuery, States state, MyObject obj, ITelegramMessage message)
        {
            if (currentQuery is CallbackQuery callbackQuery)
            {
                return _telegramBotClient.EditMessageTextAsync(obj.ChatId, callbackQuery.Message.MessageId, message.Text,
                    replyMarkup: (InlineKeyboardMarkup)message.ReplyMarkup);
            }

            return _telegramBotClient.SendTextMessageAsync(obj.ChatId, message.Text,
                replyMarkup: message.ReplyMarkup);
        }

        public Task Send<TQuery>(TQuery currentQuery, MyObject obj, ITelegramMessage[] message)
        {
            throw new NotImplementedException();
        }
    }

    public class MyObject : IFluentPagination, IFluentPaginationMenu
    {
        public string SecondName { get; set; }
        public string FirstName { get; set; }

        public long ChatId { get; set; }
        public ulong TotalCount { get; set; }
        public uint Limit { get; set; }
        public uint PageNumber { get; set; }
    }
}