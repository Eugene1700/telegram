using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Telegram.Extensions;
using System;
using System.Threading.Tasks;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Swarms = new[] { "telegram" }, Name = "global")]
    public class FluentCommandWithGlobalIntercept : FluentCommand<FluentCommandWithGlobalInterceptObject,
        FluentCommandWithGlobalInterceptStates>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public FluentCommandWithGlobalIntercept(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        protected override Task<(FluentCommandWithGlobalInterceptObject, FluentCommandWithGlobalInterceptStates)>
            Entry<TQuery>(TQuery query, FluentCommandWithGlobalInterceptObject currentObject)
        {
            return Task.FromResult((new FluentCommandWithGlobalInterceptObject
            {
                IsIntercept = false
            }, FluentCommandWithGlobalInterceptStates.Entry));
        }

        protected override IStateMachine<FluentCommandWithGlobalInterceptStates> StateMachine(
            IStateMachineBuilder<FluentCommandWithGlobalInterceptObject, FluentCommandWithGlobalInterceptStates>
                builder)
        {
            return builder.State(FluentCommandWithGlobalInterceptStates.Entry)
                .WithMessage(GetEntryMessage, Send)
                .WithCallbacks()
                .Row().NextFromCallback("Stop", "", FluentCommandWithGlobalInterceptStates.Stop, true)
                .Next(HandleMessage, true)
                .Exit<object>(FluentCommandWithGlobalInterceptStates.Stop, Stop).Build();
        }

        private async Task<ICommandExecutionResult> Stop(object arg1, FluentCommandWithGlobalInterceptStates state,
            FluentCommandWithGlobalInterceptObject arg2)
        {
            return CommandExecutionResult.Break();
        }

        public async Task<FluentCommandWithGlobalInterceptStates> HandleMessage(object query,
            FluentCommandWithGlobalInterceptStates state, FluentCommandWithGlobalInterceptObject obj)
        {
            obj.IsIntercept = !obj.IsIntercept;
            return FluentCommandWithGlobalInterceptStates.Entry;
        }

        public Task Send(object query, FluentCommandWithGlobalInterceptStates state,
            FluentCommandWithGlobalInterceptObject obj, IBotMessage message)
        {
            var chatId = query switch
            {
                QueryContext<Message> m => m.GetChatId(),
                QueryContext<CallbackQuery> callbackQuery => callbackQuery.GetChatId(),
                _ => throw new InvalidOperationException()
            };

            return _telegramBotClient.SendMessage(chatId, message.Text,
                replyMarkup: message.ToReplyMarkup());
        }

        private Task<string> GetEntryMessage(FluentCommandWithGlobalInterceptStates arg1,
            FluentCommandWithGlobalInterceptObject arg2)
        {
            return Task.FromResult("General message");
        }

        protected override async Task<GlobalInterceptResult> GlobalIntercept<TQuery>(TQuery query, FluentCommandWithGlobalInterceptObject sessionObject)
        {
            var chatId = query switch
            {
                Message m => m.Chat.Id,
                CallbackQuery callbackQuery => callbackQuery.Message.Chat.Id,
                _ => throw new InvalidOperationException()
            };
            if (sessionObject.IsIntercept)
            {
                await _telegramBotClient.SendMessage(chatId, "Intercepted message");
                sessionObject.IsIntercept = !sessionObject.IsIntercept;
                return GlobalInterceptResult.Freeze;
            }

            return GlobalInterceptResult.Next;
        }
    }

    public enum FluentCommandWithGlobalInterceptStates
    {
        Entry,
        Stop
    }

    public class FluentCommandWithGlobalInterceptObject
    {
        public bool IsIntercept { get; set; }
    }
}