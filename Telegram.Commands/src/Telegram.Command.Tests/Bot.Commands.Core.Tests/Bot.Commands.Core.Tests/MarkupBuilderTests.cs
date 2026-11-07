using Bot.Commands.Telegram.Extensions;
using Bot.Commands.Max.Models;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Max;
using Bot.Commands.Max.Services;
using Bot.Commands.Telegram;
using Bot.Commands.Telegram.Services;

namespace Bot.Commands.Core.Tests;

public class MarkupBuilderTests
{
    [Test]
    public void BuilderSelectsFileOrPhotoWithoutRetainingPreviousMode()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var builder = new MaxMarkupMessageBuilder().AddFile(bytes, "report.pdf");
        var file = builder.GetMessages().Single();
        Assert.That(file.SendAsFile, Is.True);
        Assert.That(file.FileName, Is.EqualTo("report.pdf"));
        Assert.That(file.Photo, Is.SameAs(bytes));
        var photo = builder.AddPhoto(bytes).GetMessages().Single();
        Assert.That(photo.SendAsFile, Is.False);
        Assert.That(photo.FileName, Is.Null);
    }

    [Test]
    public void MaxMessageWithoutButtonsHasNoKeyboard()
    {
        Assert.That(new BotMessage("Additional message", null, null, null).ToInlineKeyboard(), Is.Null);
        Assert.That(MaxMessageExtensions.GetResult(new Bot.Commands.Core.Services.InlineMarkupQueryBuilder()), Is.Null);
    }

    [Test]
    public void FormatsAreOwnedByTheirAdapters()
    {
        IMessageTextFormat plain = null;
        Assert.That(plain.ToParseMode(), Is.EqualTo(global::Telegram.Bot.Types.Enums.ParseMode.None));
        Assert.That(plain.ToTextFormat(), Is.Null);
        var telegram = new TelegramTextFormat(global::Telegram.Bot.Types.Enums.ParseMode.Html);
        var max = new MaxTextFormat(global::Max.Bot.Types.Enums.TextFormat.Html);
        Assert.That(telegram.ToParseMode(), Is.EqualTo(telegram.Mode));
        Assert.That(max.ToTextFormat(), Is.EqualTo(max.Mode));
        Assert.Throws<NotSupportedException>(() => telegram.ToTextFormat());
        Assert.Throws<NotSupportedException>(() => max.ToParseMode());
    }
    [Test]
    public void TelegramEscapesPlainTextButPreservesFormatting()
    {
        var message = new TelegramMarkupMessageBuilder().AppendPlain("a_b! ")
            .AppendBold("x*y").AppendUnderline("z").GetMessages().Single();
        Assert.That(message.Text, Is.EqualTo(@"a\_b\! *x\*y*__z__"));
        Assert.That(message.Format, Is.EqualTo(new TelegramTextFormat(global::Telegram.Bot.Types.Enums.ParseMode.MarkdownV2)));
    }

    [Test]
    public void MaxUsesItsOwnMarkdownDelimiters()
    {
        var message = new MaxMarkupMessageBuilder().AppendPlain("a_b! ")
            .AppendBold("x*y").AppendUnderline("z").GetMessages().Single();
        Assert.That(message.Text, Is.EqualTo(@"a\_b\! **x\*y**++z++"));
        Assert.That(message.Format, Is.EqualTo(new MaxTextFormat(global::Max.Bot.Types.Enums.TextFormat.Markdown)));
    }

    [Test]
    public void PlainVariantsEscapeConsistentlyAndPreserveWhitespace()
    {
        var builder = new TelegramMarkupMessageBuilder().AppendPlain(" ")
            .AppendPlainSpace("_").AppendPlainLine("*").AppendMarkdown("*raw*");
        Assert.That(builder.GetMessages().Single().Text, Is.EqualTo(" " + @"\_ \*" + Environment.NewLine + "*raw*"));
    }

    [Test]
    public void TelegramCodeAndLinksUseContextSpecificEscaping()
    {
        var builder = new TelegramMarkupMessageBuilder().AppendCopyContent(@"a_b\`")
            .AppendLink("https://example.com/a)", "a_b");
        Assert.That(builder.GetMessages().Single().Text,
            Is.EqualTo("`" + @"a_b\\\`" + "`" + @"[a\_b](https://example.com/a\))"));
    }

    [Test]
    public void FormattedSegmentsAreNotSplitBetweenTheirDelimiters()
    {
        var messages = new MaxMarkupMessageBuilder().AppendBold("abc").AppendUnderline("def").GetMessages(7);
        Assert.That(messages.Select(m => m.Text), Is.EqualTo(new[] { "**abc**", "++def++" }));
    }

    [Test]
    public void NativeKeyboardBuildersPreserveRowsAndCommandPayload()
    {
        var button = new CallbackDataWithCommand { Text = "go", CallbackText = "value",
            CommandDescriptor = new Bot.Commands.Abstract.Attributes.CommandAttribute { Name = "test" } };
        var url = new CallbackDataWithCommand { Text = "site", CallbackText = "https://example.com", CallbackMode = CallbackMode.Url };
        var max = new MaxInlineMarkupQueryBuilder().InlineKeyboardButtonsRow(new[] { button, url }).AddInlineKeyboardButton(button).GetResult();
        var telegram = new TelegramInlineMarkupQueryBuilder().InlineKeyboardButtonsRow(new[] { button, url }).AddInlineKeyboardButton(button).GetResult();
        Assert.That(max.Buttons.Select(r => r.Length), Is.EqualTo(new[] { 2, 1 }));
        Assert.That(telegram.InlineKeyboard.Select(r => r.Count()), Is.EqualTo(new[] { 2, 1 }));
        Assert.That(max.Buttons[0][0].Payload, Is.EqualTo("/test value"));
        Assert.That(telegram.InlineKeyboard.First().First().CallbackData, Is.EqualTo("/test value"));
        Assert.That(max.Buttons[0][1].Url, Is.EqualTo(url.CallbackText));
    }

    [Test]
    public void MaxRejectsUnsupportedButtonModes()
    {
        Assert.Throws<NotSupportedException>(() => new MaxInlineMarkupQueryBuilder()
            .AddInlineKeyboardButton(new CallbackData { Text = "pay", CallbackMode = CallbackMode.Payment }));
    }
}
