using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using Bot.Commands.Max;
using Bot.Commands.Telegram;
using Bot.Commands.Telegram.Extensions;
using Bot.Commands.Max.Models;
using SimpleHandlers.Services.Commands;
using SimpleHandlers.Services.Commands.Models;

namespace Bot.Commands.Core.Tests;

public class SharedKeyboardBuilderTests
{
    [Test]
    public void CommonContractSupportsPlainAndTypedBatchesOnEveryBuilder()
    {
        foreach (IInlineMarkupQueryBuilder builder in new IInlineMarkupQueryBuilder[]
                 { new InlineMarkupQueryBuilder(), new TelegramInlineMarkupQueryBuilder(), new MaxInlineMarkupQueryBuilder(),
                     new Bot.Commands.Telegram.Services.TelegramMarkupMessageBuilder(),
                     new Bot.Commands.Max.Services.MaxMarkupMessageBuilder() })
        {
            builder.AddInlineKeyboardButton(new CallbackData { Text = "plain", CallbackText = "plain" })
                .AddInlineKeyboardButton(Button("metadata"))
                .AddInlineKeyboardButton(new CallbackData[] { Button("a"), Button("b") })
                .AddInlineKeyboardButtons<TelegramCancelCallback, global::Telegram.Bot.Types.CallbackQuery>(
                    new CallbackData[] { Button("c"), Button("d") })
                .AddInlineKeyboardButtons<TelegramNameReplyCommand, NameReplyData, global::Telegram.Bot.Types.Message>(
                    new CallbackData[] { Button("e"), Button("f") });

            var rows = builder.GetCallbacks();
            Assert.That(rows.Select(row => row.Length), Is.EqualTo(Enumerable.Repeat(1, 8)));
            Assert.That(rows.SelectMany(row => row).Select(button => button.CallbackText),
                Is.EqualTo(new[] { "plain", "metadata", "a", "b", "c", "d", "e", "f" }));
            Assert.That(rows.Skip(4).Take(2).Select(row => row[0].CommandDescriptor.Name),
                Is.EqualTo(new[] { "cancel_callback", "cancel_callback" }));
            Assert.That(rows.Skip(6).Select(row => row[0].CommandDescriptor.Name),
                Is.EqualTo(new[] { "name_reply", "name_reply" }));
        }
    }

    [Test]
    public void CommonContractPreservesAdapterValidationAndAtomicBatchInsertion()
    {
        IInlineMarkupQueryBuilder telegram = new TelegramInlineMarkupQueryBuilder();
        Assert.Throws<Bot.Commands.Core.Exceptions.CommandsInternalException>(() =>
            telegram.AddInlineKeyboardButton(new CallbackData { Text = "long", CallbackText = new string('x', 65) }));
        Assert.Throws<Bot.Commands.Core.Exceptions.CommandsInternalException>(() =>
            telegram.AddInlineKeyboardButtons<TelegramCancelCallback, global::Telegram.Bot.Types.CallbackQuery>(
                new CallbackData[] { Button("valid"), Button(new string('x', 64)) }));
        Assert.That(telegram.GetCallbacks(), Is.Empty);

        IInlineMarkupQueryBuilder max = new MaxInlineMarkupQueryBuilder();
        max.AddInlineKeyboardButton(Button("existing"));
        Assert.Throws<NotSupportedException>(() => max.AddInlineKeyboardButton(new CallbackData[]
            { Button("valid"), Button("payment") with { CallbackMode = CallbackMode.Payment } }));
        Assert.That(max.GetCallbacks().Single().Single().CallbackText, Is.EqualTo("existing"));
    }

    [Test]
    public void MessagePreservesEmptyRowsAndButtonOrderInBothAdapters()
    {
        var builder = new InlineMarkupQueryBuilder();
        var input = new[] { Button("a"), Button("b") };
        builder.InlineKeyboardButtonsRow(Array.Empty<CallbackDataWithCommand>());
        builder.InlineKeyboardButtonsRow(input);
        builder.InlineKeyboardButtonsRow(Array.Empty<CallbackDataWithCommand>());
        builder.InlineKeyboardButtonsRow(new[] { Button("c") });
        var snapshot = builder.GetCallbacks();
        input[0] = Button("changed input");
        snapshot[1][1] = Button("changed snapshot");
        snapshot[3] = Array.Empty<CallbackDataWithCommand>();
        var message = new BotMessage("keyboard", null, null, builder.GetCallbacks());
        var expected = new[] { Array.Empty<string>(), new[] { "a", "b" }, Array.Empty<string>(), new[] { "c" } };
        Assert.That(message.ToReplyMarkup().InlineKeyboard.Select(row => row.Select(b => b.Text).ToArray()).ToArray(),
            Is.EqualTo(expected));
        Assert.That(message.ToInlineKeyboard().Buttons.Select(row => row.Select(b => b.Text).ToArray()).ToArray(),
            Is.EqualTo(expected));
    }

    [Test]
    public void BuiltMessageKeepsItsKeyboardWhenBuilderIsExtended()
    {
        var builder = new TelegramInlineMarkupQueryBuilder();
        builder.InlineKeyboardButtonsRow(new[] { Button("first") });
        var message = new BotMessage("sent", null, null, builder.GetCallbacks());
        builder.InlineKeyboardButtonsRow(new[] { Button("second"), Button("third") });
        Assert.That(message.Callbacks.Single().Select(c => c.CallbackText), Is.EqualTo(new[] { "first" }));
        Assert.That(builder.GetCallbacks().Select(row => row.Length), Is.EqualTo(new[] { 1, 2 }));
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
            Assert.That(builder.GetCallbacks().Select(row => row.Length), Is.EqualTo(new[] { 2, 1, 1 }));
            Assert.That(builder.GetCallbacks()[1][0].CommandDescriptor.Name, Is.EqualTo("cancel_callback"));
            Assert.That(builder.GetCallbacks()[2][0].CommandDescriptor.Name, Is.EqualTo("name_reply"));
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
        Assert.That(max.GetCallbacks().Select(row => row.Length), Is.EqualTo(new[] { 1, 1 }));
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
        Assert.That(builder.GetCallbacks().Select(row => row.Length), Is.EqualTo(new[] { 0, 1 }));
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
        Assert.That(builder.GetCallbacks(), Has.Length.EqualTo(4));
    }

    private static CallbackDataWithCommand Button(string data) => new() { Text = data, CallbackText = data };
}
