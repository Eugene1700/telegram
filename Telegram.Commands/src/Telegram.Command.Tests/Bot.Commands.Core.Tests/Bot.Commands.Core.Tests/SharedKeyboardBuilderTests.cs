using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using Bot.Commands.Max;
using Bot.Commands.Telegram;
using SimpleHandlers.Services.Commands;
using SimpleHandlers.Services.Commands.Models;

namespace Bot.Commands.Core.Tests;

public class SharedKeyboardBuilderTests
{
    [Test]
    public void BuiltMessageKeepsItsKeyboardWhenBuilderIsExtended()
    {
        var builder = new TelegramInlineMarkupQueryBuilder();
        builder.InlineKeyboardButtonsRow(new[] { Button("first") });
        var message = new BotMessage("sent", null, null, builder.GetCallbacks());
        builder.InlineKeyboardButtonsRow(new[] { Button("second"), Button("third") });
        Assert.That(message.Callbacks.Select(c => c.CallbackText), Is.EqualTo(new[] { "first" }));
        Assert.That(builder.GetCallbacks().Select(c => c.Row), Is.EqualTo(new[] { 0, 1, 1 }));
        Assert.That(builder.GetResult().InlineKeyboard.Select(r => r.Count()), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void AllAdaptersExposeTheCommonContractAndPreserveMetadata()
    {
        foreach (IInlineMarkupQueryBuilder builder in new IInlineMarkupQueryBuilder[]
                 { new InlineMarkupQueryBuilder(), new TelegramInlineMarkupQueryBuilder(), new MaxInlineMarkupQueryBuilder() })
        {
            builder.InlineKeyboardButtonsRow(new[] { Button("a"), Button("b") });
            builder.AddInlineKeyboardButton<TelegramCancelCallback, global::Telegram.Bot.Types.CallbackQuery>(Button("command"));
            builder.AddInlineKeyboardButton<TelegramNameReplyCommand, NameReplyData, global::Telegram.Bot.Types.Message>(Button("session"));
            Assert.That(builder.GetCallbacks().Select(c => c.Row), Is.EqualTo(new[] { 0, 0, 1, 2 }));
            Assert.That(builder.GetCallbacks()[2].CommandDescriptor.Name, Is.EqualTo("cancel_callback"));
            Assert.That(builder.GetCallbacks()[3].CommandDescriptor.Name, Is.EqualTo("name_reply"));
        }
    }

    [Test]
    public void TypedAdapterOverloadsAndArrayLayoutStillWork()
    {
        var telegram = new TelegramInlineMarkupQueryBuilder()
            .AddInlineKeyboardButtons<TelegramCancelCallback>(new CallbackData[] { Button("a"), Button("b") })
            .AddInlineKeyboardButton<TelegramNextPhotoCallback, List<SimpleHandlers.Services.Commands.Models.PhotoSessionObject>>(Button("session"));
        var max = new MaxInlineMarkupQueryBuilder()
            .AddInlineKeyboardButtons<MaxCancelCallback>(new CallbackData[] { Button("a"), Button("b") });
        Assert.That(telegram.GetResult().InlineKeyboard.Take(2).SelectMany(r => r).Select(b => b.CallbackData),
            Is.EqualTo(new[] { "/cancel_callback a", "/cancel_callback b" }));
        Assert.That(max.GetResult().Buttons.SelectMany(r => r).Select(b => b.Payload),
            Is.EqualTo(new[] { "/cancel_callback a", "/cancel_callback b" }));
        Assert.That(max.GetCallbacks().Select(b => b.Row), Is.EqualTo(new[] { 0, 1 }));
    }

    [Test]
    public void UnsupportedBatchDoesNotPartiallyChangeBuilderAndEmptyRowsSurvive()
    {
        var builder = new MaxInlineMarkupQueryBuilder();
        builder.InlineKeyboardButtonsRow(Array.Empty<CallbackDataWithCommand>());
        Assert.Throws<NotSupportedException>(() => builder.AddInlineKeyboardButton(new CallbackData[]
            { Button("first"), Button("unsupported") with { CallbackMode = CallbackMode.Payment } }));
        builder.AddInlineKeyboardButton(Button("last"));
        Assert.That(builder.GetResult().Buttons.Select(row => row.Length), Is.EqualTo(new[] { 0, 1 }));
        Assert.That(builder.GetCallbacks().Single().Row, Is.EqualTo(1));
    }

    [Test]
    public void TelegramSpecificModesAndLegacyLengthValidationRemain()
    {
        var builder = new TelegramInlineMarkupQueryBuilder();
        foreach (var mode in new[] { CallbackMode.Inline, CallbackMode.InlineCurrent, CallbackMode.Payment, CallbackMode.Url })
            builder.AddInlineKeyboardButton(Button("https://example.com") with { CallbackMode = mode });
        var buttons = builder.GetResult().InlineKeyboard.SelectMany(row => row).ToArray();
        Assert.That(buttons[0].SwitchInlineQuery, Is.EqualTo("https://example.com"));
        Assert.That(buttons[1].SwitchInlineQueryCurrentChat, Is.EqualTo("https://example.com"));
        Assert.That(buttons[2].Pay, Is.True);
        Assert.That(buttons[3].Url, Is.EqualTo("https://example.com"));
        Assert.Throws<Bot.Commands.Core.Exceptions.CommandsInternalException>(() =>
            builder.AddInlineKeyboardButton(new CallbackData { Text = "long", CallbackText = new string('x', 65) }));
        Assert.That(builder.GetCallbacks(), Has.Count.EqualTo(4));
    }

    private static CallbackDataWithCommand Button(string data) => new() { Text = data, CallbackText = data };
}
