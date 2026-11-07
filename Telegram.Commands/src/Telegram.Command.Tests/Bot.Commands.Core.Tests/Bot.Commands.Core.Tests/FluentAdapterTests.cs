using global::Telegram.Bot.Types;
using Bot.Commands.Max.Models;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Telegram.Extensions;
using Bot.Commands.Telegram.Models;

namespace Bot.Commands.Core.Tests;

public class FluentAdapterTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task FluentPreservesAttachmentSendingMode(bool sendAsFile)
    {
        var content = new byte[] { 1, 2, 3 };
        var source = new BotMessage("caption", content, null, null)
        {
            SendAsFile = sendAsFile, FileName = "image.png"
        };
        IBotMessage sent = null;
        var builder = new StateMachineBuilder<object, int>();
        builder.State(0).WithMessage<Message>((_, _) => Task.FromResult<ITextMessage>(source),
            (_, _, _, message) => { sent = message; return Task.CompletedTask; });
        var machine = (StateMachine<object, int>)builder.Build();
        await machine.GetStateInternal(0).SendMessages(
            new TelegramMessageQueryContext(new Message { Chat = new Chat { Id = 12 } }), null);
        Assert.That(sent.Photo, Is.SameAs(content));
        Assert.That(sent.SendAsFile, Is.EqualTo(sendAsFile));
        Assert.That(sent.FileName, Is.EqualTo("image.png"));
        Assert.That(sent.Text, Is.EqualTo("caption"));
    }

    [Test]
    public async Task GenericExitButtonsPreserveTargetAndPayloadAcrossBuilderKinds()
    {
        var builder = new StateMachineBuilder<object, int>();
        var messages = new List<IBotMessage>();
        Task Send(IQueryContext<object> query, int state, object data, IBotMessage message)
        {
            messages.Add(message);
            return Task.CompletedTask;
        }
        builder.State(0)
            .WithMessage((_, _) => Task.FromResult<ITextMessage>(new BotMessage("main", null, null, null)), Send)
            .WithCallbacks().Row()
            .ExitFromCallback<SimpleHandlers.Services.Commands.CancelCallback>("Cancel", "static")
            .ExitFromCallback<SimpleHandlers.Services.Commands.CancelCallback>(new CallbackData { Text = "Cancel", CallbackText = "data" })
            .ExitFromCallback<SimpleHandlers.Services.Commands.CancelCallback>((state, _) => new CallbackData { Text = "Cancel", CallbackText = state.ToString() })
            .KeyBoard((_, _, keyboard) =>
            {
                keyboard.Row().ExitFromCallback<SimpleHandlers.Services.Commands.CancelCallback>("Cancel", "keyboard");
                return Task.CompletedTask;
            })
            .WithMessages((_, _, flow) =>
            {
                flow.WithMessage((_, _) => Task.FromResult<ITextMessage>(new BotMessage("dynamic", null, null, null)), Send)
                    .WithCallbacks().Row().ExitFromCallback<SimpleHandlers.Services.Commands.CancelCallback>("Cancel", "dynamic");
                return Task.CompletedTask;
            });
        var machine = (StateMachine<object, int>)builder.Build();
        await machine.GetStateInternal(0).SendMessages(new TelegramMessageQueryContext(new Message { Chat = new Chat { Id = 12 } }), null);
        var buttons = messages.SelectMany(message => message.Callbacks).ToArray();
        Assert.That(buttons.Select(button => button.CallbackText), Is.EqualTo(new[] { "static", "data", "0", "keyboard", "dynamic" }));
        Assert.That(buttons.All(button => button.CommandDescriptor.Name == BotCommandExtensions.GetCommandInfo(typeof(SimpleHandlers.Services.Commands.CancelCallback)).Name), Is.True);
    }

    [Test]
    public async Task SenderTransitionAndFinalizerReceiveOriginalTypedContext()
    {
        var builder = new StateMachineBuilder<object, int>();
        var native = new Message { Text = "hello", Chat = new Chat { Id = 12 } };
        var context = new TelegramMessageQueryContext(native);
        object sent = null;
        object handled = null;
        builder.State(0)
            .WithMessage<Message>((s, o) => Task.FromResult<ITextMessage>(new BotMessage("hi", null, null, null)),
                (q, s, o, m) => { sent = q; return Task.CompletedTask; })
            .Next<Message>((q, s, o) => { handled = q; return Task.FromResult(1); }, false)
            .Exit<Message>(1, (q, s, o) =>
            {
                Assert.That(q, Is.SameAs(context));
                return Task.FromResult<Bot.Commands.Abstract.Interfaces.ICommandExecutionResult>(CommandExecutionResult.Break());
            });
        var machine = (StateMachine<object, int>)builder.Build();
        await machine.GetStateInternal(0).SendMessages(context, null);
        var (next, _) = await machine.GetStateInternal(0).HandleQuery(context, null);
        await machine.GetStateInternal(next).Finalize(context, null);
        Assert.That(sent, Is.SameAs(context));
        Assert.That(handled, Is.SameAs(context));
    }

    [Test]
    public async Task DynamicMessageAndCallbackUseCorrectQueryObjects()
    {
        var builder = new StateMachineBuilder<object, int>();
        IBotMessage sentMessage = null;
        IQueryContext<CallbackQuery> handled = null;
        builder.State(0).WithMessages((s, o, messages) =>
        {
            messages.WithMessage<CallbackQuery>((_, _) => Task.FromResult<ITextMessage>(new BotMessage("hi", null, null, null)),
                    (q, _, _, m) => { sentMessage = m; return Task.CompletedTask; })
                .WithCallbacks().Row().OnCallback<CallbackQuery>((_, _) => new CallbackData { Text = "go", CallbackText = "value" },
                    (q, _, _, data) => { handled = q; Assert.That(data, Is.EqualTo("value")); return Task.FromResult(1); }, false);
            return Task.CompletedTask;
        });
        var machine = (StateMachine<object, int>)builder.Build();
        var native = new CallbackQuery { Message = new Message { Chat = new Chat { Id = 1 } }, From = new User { Id = 2 } };
        var context = new CallbackQueryContext(native);
        await machine.GetStateInternal(0).SendMessages(context, null);
        native.Data = sentMessage.Callbacks.Single().CallbackText;
        var (next, _) = await machine.GetStateInternal(0).HandleQuery(context, null);
        Assert.That(next, Is.EqualTo(1));
        Assert.That(handled, Is.SameAs(context));
    }

    [Test]
    public void TelegramKeyboardPreservesCommonRows()
    {
        var builder = new InlineMarkupQueryBuilder();
        builder.InlineKeyboardButtonsRow(new[] { Button("a"), Button("b") });
        builder.InlineKeyboardButtonsRow(new[] { Button("c") });
        var markup = new BotMessage("hi", null, null, builder.GetCallbacks()).ToReplyMarkup();
        Assert.That(markup.InlineKeyboard.Select(row => row.Select(b => b.Text).ToArray()).ToArray(),
            Is.EqualTo(new[] { new[] { "a", "b" }, new[] { "c" } }));
    }

    private static CallbackDataWithCommand Button(string text) => new() { Text = text, CallbackText = text };

    [Test]
    public void MaxKeyboardPreservesRowsAndCommandPayload()
    {
        var builder = new InlineMarkupQueryBuilder();
        builder.InlineKeyboardButtonsRow(new[] { Button("a"), Button("b") });
        builder.InlineKeyboardButtonsRow(new[] { Button("c") with
        {
            CommandDescriptor = new Bot.Commands.Abstract.Attributes.CommandAttribute { Name = "test" }
        }});
        var markup = new BotMessage("hi", null, null, builder.GetCallbacks()).ToInlineKeyboard();
        Assert.That(markup.Buttons.Select(row => row.Length), Is.EqualTo(new[] { 2, 1 }));
        Assert.That(markup.Buttons[1][0].Payload, Is.EqualTo("/test c"));
    }
}
