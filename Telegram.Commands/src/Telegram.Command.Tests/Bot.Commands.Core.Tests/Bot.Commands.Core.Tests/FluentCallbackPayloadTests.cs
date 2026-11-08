using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.Telegram.Models;
using global::Telegram.Bot.Types;

namespace Bot.Commands.Core.Tests;

public class FluentCallbackPayloadTests
{
    [TestCase("")]
    [TestCase("a=b")]
    [TestCase("a&b")]
    [TestCase("a&fid=foreign&fud=wrong")]
    [TestCase("100% + имя")]
    public async Task CallbackRoundTripPreservesUserPayload(string payload)
    {
        IBotMessage sent = null;
        string received = null;
        var builder = new StateMachineBuilder<object, int>();
        builder.State(0)
            .WithMessage<Message>((_, _) => Task.FromResult<ITextMessage>(new BotMessage("choose", null, null, null)),
                (_, _, _, message) => { sent = message; return Task.CompletedTask; })
            .WithCallbacks().Row().OnCallback<CallbackQuery>(
                (_, _) => new CallbackData { Text = "Choose", CallbackText = payload },
                (_, _, _, value) => { received = value; return Task.FromResult(1); }, false);
        builder.Exit<CallbackQuery>(1, (_, _, _) =>
                Task.FromResult<Bot.Commands.Abstract.Interfaces.ICommandExecutionResult>(CommandExecutionResult.Break()));
        var machine = (StateMachine<object, int>)builder.Build();
        await machine.GetStateInternal(0).SendMessages(
            new TelegramMessageQueryContext(new Message { Chat = new Chat { Id = 12 } }), null);
        var callback = new CallbackQuery
        {
            Data = sent.Callbacks.SelectMany(row => row).Single().CallbackText,
            Message = new Message { Chat = new Chat { Id = 12 } }
        };
        var (next, _) = await machine.GetStateInternal(0).HandleQuery(new CallbackQueryContext(callback), null);
        Assert.That(next, Is.EqualTo(1));
        Assert.That(received, Is.EqualTo(payload));
    }

    [TestCase("fidelity")]
    [TestCase("fidother=value")]
    public async Task OrdinaryTextWithSimilarPrefixReachesMessageHandler(string text)
    {
        var builder = new StateMachineBuilder<object, int>();
        builder.State(0).Next<Message>((context, _, _) =>
        {
            Assert.That(context.GetData(), Is.EqualTo(text));
            return Task.FromResult(1);
        }, false);
        var machine = (StateMachine<object, int>)builder.Build();
        var (next, _) = await machine.GetStateInternal(0).HandleQuery(
            new TelegramMessageQueryContext(new Message { Text = text, Chat = new Chat { Id = 12 } }), null);
        Assert.That(next, Is.EqualTo(1));
    }
}
