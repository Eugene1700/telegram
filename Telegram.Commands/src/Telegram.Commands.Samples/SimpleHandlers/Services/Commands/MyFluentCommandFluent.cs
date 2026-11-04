using Bot.Commands.Abstract.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.ReplyMarkups;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;
using Bot.Commands.Telegram.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Telegram;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.UI.Pagination;

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

    [Command(Swarms = new[] { "telegram" }, Name = "myfluent")]
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
                Message message => message.Chat.Id,
                CallbackQuery callbackQuery => callbackQuery.Message.Chat.Id,
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
                    BotCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>())
                .ExitFromCallback(CallbackDataWithCommand())
                .KeyBoard(SameCallbackKey)
                .KeyboardWithPagination<MyObject, States, CallbackQuery>(Paginator)
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
        private Task SendWithParseMode(object currentQuery, States arg2, MyObject obj, IBotMessage message)
        {
            if (currentQuery is QueryContext<CallbackQuery> callbackContext)
            {
                return _telegramBotClient.EditMessageText(obj.ChatId, callbackContext.Query.Message.MessageId, message.Text,
                    message.ParseMode.ToParseMode(),
                    replyMarkup: (InlineKeyboardMarkup)message.ToReplyMarkup());
            }

            return _telegramBotClient.SendMessage(obj.ChatId, message.Text,
                message.ParseMode.ToParseMode(),
                replyMarkup: message.ToReplyMarkup());
        }

        private Task<ITextMessage> GetMessageWithParseMode(States arg1, MyObject arg2)
        {
            throw new NotImplementedException();
        }
        
        private Task<ITextMessage> GetMessageWithParseMode2(States arg1, MyObject arg2)
        {
            return Task.FromResult<ITextMessage>(new BotMessage("*MarkDownText*", null, BotMessageParseMode.MarkDown, null));
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

        private Task SendSubMessage(object currentQuery, States state, MyObject obj, IBotMessage message)
        {
            return _telegramBotClient.SendMessage(obj.ChatId, message.Text,
                replyMarkup: message.ToReplyMarkup());
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
                CommandDescriptor = BotCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>()
            };
        }

        private static CallbackData KeyBoardBuild(States state, MyObject obj)
        {
            return new CallbackDataWithCommand
            {
                Text = "Another cancel",
                CallbackText = "data",
                CommandDescriptor = BotCommandExtensions.GetCommandInfo<CancelCallback, CallbackQuery>()
            };
        }

        private async Task<States> SendUserDataHandler(CallbackQuery arg1, States states, MyObject arg2, string userData)
        {
            await _telegramBotClient.SendMessage(arg2.ChatId, userData);
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

        private async Task<ICommandExecutionResult> Finalize(object currentQuery, States  state, MyObject obj)
        {
            long chatId = 0;
            if (currentQuery is Message message)
            {
                chatId = message.Chat.Id;
            }

            if (currentQuery is CallbackQuery callbackQuery)
            {
                chatId = callbackQuery.Message.Chat.Id;
            }

            await _telegramBotClient.SendMessage(chatId, $"Your data: {obj.FirstName} {obj.SecondName}");
            return CommandExecutionResult.Break();
        }

        public Task Send(object currentQuery, States state, MyObject obj, IBotMessage message)
        {
            if (currentQuery is QueryContext<CallbackQuery> callbackContext)
            {
                return _telegramBotClient.EditMessageText(obj.ChatId, callbackContext.Query.Message.MessageId, message.Text,
                    replyMarkup: (InlineKeyboardMarkup)message.ToReplyMarkup());
            }

            return _telegramBotClient.SendMessage(obj.ChatId, message.Text,
                replyMarkup: message.ToReplyMarkup());
        }

        public Task Send<TQuery>(TQuery currentQuery, MyObject obj, IBotMessage[] message)
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
