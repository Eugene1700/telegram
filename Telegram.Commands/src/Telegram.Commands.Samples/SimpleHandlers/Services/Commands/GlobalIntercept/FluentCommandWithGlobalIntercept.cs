using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using System;
using System.Threading.Tasks;
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
    [Command(Name = "global")]
    public abstract class FluentCommandWithGlobalIntercept : FluentCommand<FluentCommandWithGlobalInterceptObject,
        FluentCommandWithGlobalInterceptStates>
    {
        private readonly ISampleMessageSender _sender;

        public FluentCommandWithGlobalIntercept(ISampleMessageSender telegramBotClient)
        {
            _sender = telegramBotClient;
        }

        protected override Task<(FluentCommandWithGlobalInterceptObject, FluentCommandWithGlobalInterceptStates)>
            Entry<TQuery>(IQueryContext<TQuery> query, FluentCommandWithGlobalInterceptObject currentObject)
        {
            return Task.FromResult((new FluentCommandWithGlobalInterceptObject
            {
                IsIntercept = false, ChatId = SampleQuery.ChatId(query)
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
                .Next<object>(HandleMessage, true)
                .Exit<object>(FluentCommandWithGlobalInterceptStates.Stop, Stop).Build();
        }

        private async Task<ICommandExecutionResult> Stop<TQuery>(TQuery arg1, FluentCommandWithGlobalInterceptStates state,
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
            var chatId = obj.ChatId;

            return _sender.Send(chatId, message);
        }

        private Task<string> GetEntryMessage(FluentCommandWithGlobalInterceptStates arg1,
            FluentCommandWithGlobalInterceptObject arg2)
        {
            return Task.FromResult("General message");
        }

        protected override async Task<GlobalInterceptResult> GlobalIntercept<TQuery>(IQueryContext<TQuery> query, FluentCommandWithGlobalInterceptObject sessionObject)
        {
            var chatId = sessionObject.ChatId;
            if (sessionObject.IsIntercept)
            {
                await _sender.Send(chatId, SampleMessages.Text("Intercepted message"));
                sessionObject.IsIntercept = !sessionObject.IsIntercept;
                return GlobalInterceptResult.Freeze;
            }

            return GlobalInterceptResult.Next;
        }
    }
}
