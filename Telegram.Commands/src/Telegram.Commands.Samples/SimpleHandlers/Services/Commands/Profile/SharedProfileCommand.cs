using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands;

[Command(Name = "profile", Authorized = false)]
public abstract class SharedProfileCommand(ISampleMessageSender sender) : FluentCommand<SharedProfileData, SharedProfileStep>
{
    protected override Task<(SharedProfileData, SharedProfileStep)> Entry<TQuery>(IQueryContext<TQuery> query, SharedProfileData data)
    {
        var chatId = query.GetChatId();
        return Task.FromResult((new SharedProfileData { ChatId = chatId }, SharedProfileStep.Name));
    }

    protected override IStateMachine<SharedProfileStep> StateMachine(IStateMachineBuilder<SharedProfileData, SharedProfileStep> builder)
        => builder.State(SharedProfileStep.Name)
            .WithMessage("What is your name? Send text or choose the default.", Send)
            .WithCallbacks().Row().OnCallback<object>((state, data) => new CallbackData
            {
                Text = "Use Alex", CallbackText = "Alex"
            }, (q, state, data, value) =>
            {
                data.Name = value;
                return Task.FromResult(SharedProfileStep.Done);
            }, true)
            .Next<object>((query, state, data) =>
            {
                data.Name = query.GetData();
                return Task.FromResult(string.IsNullOrWhiteSpace(data.Name) ? SharedProfileStep.Name : SharedProfileStep.Done);
            }, true)
            .Exit<object>(SharedProfileStep.Done, Finish).Build();

    private Task Send(object query, SharedProfileStep state, SharedProfileData data, IBotMessage message)
        => sender.Send(data.ChatId, message);

    private async Task<ICommandExecutionResult> Finish(object query, SharedProfileStep state, SharedProfileData data)
    {
        await sender.Send(data.ChatId, SampleMessages.Text($"Profile saved: {data.Name}"));
        return CommandExecutionResult.Break();
    }
}
