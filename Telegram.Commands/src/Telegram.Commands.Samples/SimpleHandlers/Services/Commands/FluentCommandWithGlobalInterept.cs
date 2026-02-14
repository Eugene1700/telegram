using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders.Extensions;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Messages;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "global")]
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

        private async Task<ITelegramCommandExecutionResult> Stop(object arg1, FluentCommandWithGlobalInterceptStates state, 
            FluentCommandWithGlobalInterceptObject arg2)
        {
            return TelegramCommandExecutionResult.Break();
        }

        public async Task<FluentCommandWithGlobalInterceptStates> HandleMessage(object query,
            FluentCommandWithGlobalInterceptStates state, FluentCommandWithGlobalInterceptObject obj)
        {
            obj.IsIntercept = !obj.IsIntercept;
            return FluentCommandWithGlobalInterceptStates.Entry;
        }

        public Task Send<TQuery>(TQuery query, FluentCommandWithGlobalInterceptStates state,
            FluentCommandWithGlobalInterceptObject obj, ITelegramMessage message)
        {
            var chatId = query switch
            {
                Message m => m.GetChatId(),
                CallbackQuery callbackQuery => callbackQuery.GetChatId(),
                _ => throw new InvalidOperationException()
            };

            return _telegramBotClient.SendTextMessageAsync(chatId, message.Text,
                replyMarkup: message.ReplyMarkup);
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
                Message m => m.GetChatId(),
                CallbackQuery callbackQuery => callbackQuery.GetChatId(),
                _ => throw new InvalidOperationException()
            };
            if (sessionObject.IsIntercept)
            {
                await _telegramBotClient.SendTextMessageAsync(chatId, "Intercepted message");
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