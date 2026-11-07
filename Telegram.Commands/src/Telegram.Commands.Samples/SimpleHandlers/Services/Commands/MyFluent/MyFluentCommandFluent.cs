using Bot.Commands.Abstract.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.UI.Pagination;

namespace SimpleHandlers.Services.Commands
{

    [Command(Name = "myfluent")]
    public abstract class MyFluentCommandFluent<TMessage, TCallback> : FluentCommand<MyObject, States>
        where TMessage : class
        where TCallback : class
    {
        private readonly ISampleMessageSender _sender;

        public MyFluentCommandFluent(ISampleMessageSender telegramBotClient)
        {
            _sender = telegramBotClient;
        }

        protected override Task<(MyObject, States)> Entry<TQuery>(IQueryContext<TQuery> query, MyObject myObject)
        {
            var chatId = SampleQuery.ChatId(query);
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
                .Row().OnCallback<MyObject, States, TCallback>("Default Name (Jack)", "Jack", FirstNameCallbackHandler,
                    true)
                .Row().NextFromCallback("Skip", "data", States.Surname, true)
                .Row().NextFromCallback( "Exit", "data", States.Exit, true)
                .Row().OnCallback<MyObject, States, TCallback>("Send TEXT", "TEXT", SendUserDataHandler, false)
                .Row().NextFromCallback("Message with break", "", States.Break, false)
                .Row().ExitFromCallback<CancelCallback>("Cancel", "someData")
                .Row().ExitFromCallback<CancelCallback>("Cancel", "someData")
                .Row().ExitFromCallback<CancelCallback>(KeyBoardBuild)
                .ExitFromCallback<CancelCallback>(CancelButton())
                .KeyBoard(SameCallbackKey)
                .KeyboardWithPagination<MyObject, States, TCallback>(Paginator)
                .WithMessages(OtherMessages)
                // .WithMessage(GetMessageWithParseMode, SendWithParseMode)
                .WithMessage(GetMessageWithParseMode2, SendWithParseMode)
                .Next<TMessage>(FirstNameMessageHandler, true)
                .State(States.Surname)
                .WithMessage((state, obj) => Task.FromResult($"Ok, send me your surname, {obj.FirstName}"), Send)
                .WithCallbacks().KeyBoard(GetSurnameKeyboard)
                .Next<TMessage>(SecondNameHandler, true)
                .State(States.Validate)
                .WithMessage("Your name is too short! Please, send me again", Send)
                .WithCallbacks()
                .Row().NextFromCallback("Skip", "data", States.Surname, true)
                .Next<TMessage>(FirstNameMessageHandler, true)
                .State(States.Break)
                .WithMessage("Message without waiting answer", Send)
                .FireAndForget()
                .Exit<object>(States.Exit, Finalize);

            return a.Build();
        }
        private Task SendWithParseMode(IQueryContext<object> currentQuery, States state, MyObject obj, IBotMessage message)
            => Send(currentQuery, state, obj, message);
        private Task<ITextMessage> GetMessageWithParseMode(States arg1, MyObject arg2)
        {
            return GetMessageWithParseMode2(arg1, arg2);
        }
        
        private Task<ITextMessage> GetMessageWithParseMode2(States arg1, MyObject arg2)
        {
            return Task.FromResult<ITextMessage>(new BotMessage("Additional message", null, null, null));
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

        private Task SendSubMessage(IQueryContext<object> currentQuery, States state, MyObject obj, IBotMessage message)
        {
            return _sender.Send(obj.ChatId, message);
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
                b.OnCallback<MyObject, States, TCallback>($"{num}", $"{num}", SendUserDataHandler, false);
            }

            return Task.FromResult<IFluentPaginationMenu>(arg1);
        }

        private Task SameCallbackKey(States state, MyObject arg1, ICallbacksBuilderBase<MyObject, States> arg2)
        {
            var b = arg2.Row();
            foreach (var num in Enumerable.Range(0, 5))
            {
                b.OnCallback<MyObject, States, TCallback>($"{num}", $"{num}", SendUserDataHandler, false);
            }

            return Task.CompletedTask;
        }


        private Task GetSurnameKeyboard(States state, MyObject arg1, ICallbacksBuilderBase<MyObject, States> arg2)
        {
            arg2.Row().NextFromCallback($"Skip for {state}", arg1.FirstName, States.Exit, true)
                .NextFromCallback("Back", arg1.FirstName, States.Name, true)
                .OnCallback<MyObject, States, TCallback>("Default SecondName Smith", "Smith",
                    SecondNameCallbackHandler, true)
                .Row().NextFromCallback("Finish", arg1.FirstName, States.Exit, true);
            return Task.CompletedTask;
        }

        private static CallbackData CancelButton()
        {
            return new CallbackData
            {
                Text = "Another another cancel",
                CallbackText = "data"
            };
        }

        private static CallbackData KeyBoardBuild(States state, MyObject obj)
        {
            return new CallbackData
            {
                Text = "Another cancel",
                CallbackText = "data"
            };
        }

        private async Task<States> SendUserDataHandler(IQueryContext<TCallback> arg1, States states, MyObject arg2, string userData)
        {
            await _sender.Send(arg2.ChatId, SampleMessages.Text(userData));
            return States.Name;
        }

        private static async Task<States> FirstNameCallbackHandler(IQueryContext<TCallback> query, States state, MyObject obj, string userData)
        {
            return await FirstNameHandler(userData, obj);
        }

        private async Task<States> SecondNameCallbackHandler(IQueryContext<TCallback> query, States state, MyObject obj, string userData)
        {
            return await SecondNameHandler(userData, obj);
        }

        private static async Task<States> FirstNameMessageHandler(IQueryContext<TMessage> query, States state, MyObject obj)
        {
            return await FirstNameHandler(query.GetData(), obj);
        }

        private static async Task<States> FirstNameHandler(string text, MyObject obj)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
            {
                return States.Validate;
            }

            obj.FirstName = text;
            return States.Surname;
        }


        public async Task<States> SecondNameHandler(IQueryContext<TMessage> message, States states, MyObject obj)
        {
            return await SecondNameHandler(message.GetData(), obj);
        }

        public async Task<States> SecondNameHandler(string text, MyObject obj)
        {
            obj.SecondName = text;
            return States.Exit;
        }

        private async Task<ICommandExecutionResult> Finalize(IQueryContext<object> currentQuery, States  state, MyObject obj)
        {
            await _sender.Send(obj.ChatId, SampleMessages.Text($"Your data: {obj.FirstName} {obj.SecondName}"));
            return CommandExecutionResult.Break();
        }

        public Task Send(IQueryContext<object> currentQuery, States state, MyObject obj, IBotMessage message)
        {
            var messageId = SampleQuery.CallbackMessageId(currentQuery);
            return messageId == null ? _sender.Send(obj.ChatId, message) : _sender.Edit(obj.ChatId, messageId, message);
        }
        public Task Send<TQuery>(TQuery currentQuery, MyObject obj, IBotMessage[] message)
        {
            return Task.WhenAll(message.Select(item => _sender.Send(obj.ChatId, item)));
        }
    }
}
