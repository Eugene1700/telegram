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
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;

namespace SimpleHandlers.Services.Commands;

public static class SampleMessages
{
    public static IBotMessage Text(string text) => new BotMessage(text, null, BotMessageParseMode.Plain, null);
}

[Command(Name = "ask", Authorized = false)]
public sealed class AskCommand(ISampleMessageSender sender) : IQueryCommand<Tg.Message>, IQueryCommand<Mx.Message>
{
    public async Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("What should I call you?"));
        return CommandExecutionResult.Ahead<NameReplyCommand, Tg.Message, NameReplyData>(new NameReplyData());
    }

    public async Task<ICommandExecutionResult> Execute(QueryContext<Mx.Message> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("What should I call you?"));
        return CommandExecutionResult.Ahead<NameReplyCommand, Mx.Message, NameReplyData>(new NameReplyData());
    }
}

public sealed class NameReplyData { }

[Command(Name = "name_reply", Authorized = false)]
public sealed class NameReplyCommand(ISampleMessageSender sender) : ISessionCommand<Tg.Message, NameReplyData>, ISessionCommand<Mx.Message, NameReplyData>
{
    public Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query, NameReplyData data) => Reply(query.GetChatId(), query.GetData());
    public Task<ICommandExecutionResult> Execute(QueryContext<Mx.Message> query, NameReplyData data) => Reply(query.GetChatId(), query.GetData());

    private async Task<ICommandExecutionResult> Reply(long chatId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await sender.Send(chatId, SampleMessages.Text("Please send your name as text."));
            return CommandExecutionResult.Freeze();
        }
        await sender.Send(chatId, SampleMessages.Text($"Nice to meet you, {text}!"));
        return CommandExecutionResult.Break();
    }
}

public enum SharedProfileStep { Name, Done }
public sealed class SharedProfileData
{
    public long ChatId { get; set; }
    public string Name { get; set; }
}

[Command(Name = "profile", Authorized = false)]
public sealed class SharedProfileCommand(ISampleMessageSender sender) : FluentCommand<SharedProfileData, SharedProfileStep>
{
    protected override Task<(SharedProfileData, SharedProfileStep)> Entry<TQuery>(TQuery query, SharedProfileData data)
    {
        var chatId = query switch
        {
            Tg.Message m => m.Chat.Id,
            Mx.Message m => m.Recipient.ChatId.Value,
            _ => throw new InvalidOperationException("Start /profile with a message.")
        };
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
                data.Name = query switch { Tg.Message m => m.Text, Mx.Message m => m.Text, _ => null };
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
