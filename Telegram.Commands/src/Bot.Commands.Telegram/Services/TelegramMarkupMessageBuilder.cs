using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using Bot.Commands.Telegram.Extensions;

namespace Bot.Commands.Telegram.Services;

/// <summary>Builds messages using Telegram MarkdownV2 syntax.</summary>
public class TelegramMarkupMessageBuilder : MessageBuilderBase
{
    protected override BotMessageParseMode ParseMode => BotMessageParseMode.MarkDown;
    protected override string PrepareText(string text) => text.ReplaceMarkDownEscapeSymbols();

    /// <summary>Appends trusted, already formatted Markdown without escaping.</summary>
    public TelegramMarkupMessageBuilder AppendMarkdown(string text) { Append(text, false); return this; }
    public new TelegramMarkupMessageBuilder AppendPlain(string text) { base.AppendPlain(text); return this; }
    public new TelegramMarkupMessageBuilder AppendPlainLine(string text) { base.AppendPlainLine(text); return this; }
    public new TelegramMarkupMessageBuilder AppendPlainSpace(string text) { base.AppendPlainSpace(text); return this; }
    public new TelegramMarkupMessageBuilder AppendEmptyLine() { base.AppendEmptyLine(); return this; }

    public TelegramMarkupMessageBuilder AppendBold(string text) => AppendMarkdown("*" + PrepareText(text) + "*");
    public TelegramMarkupMessageBuilder AppendUnderline(string text) => AppendMarkdown("__" + PrepareText(text) + "__");
    public TelegramMarkupMessageBuilder AppendCopyContent(string text) => AppendMarkdown("`" + (text ?? string.Empty).Replace(@"\", @"\\").Replace("`", @"\`") + "`");
    public TelegramMarkupMessageBuilder AppendLink(string link, string title) => AppendMarkdown("[" + PrepareText(title) + "](" + link.Replace(@"\", @"\\").Replace(")", @"\)") + ")");
    public TelegramMarkupMessageBuilder AppendBoldLine(string text) => AppendBold(text).AppendEmptyLine();
    public TelegramMarkupMessageBuilder AppendUnderlineLine(string text) => AppendUnderline(text).AppendEmptyLine();
    public TelegramMarkupMessageBuilder AppendCopyContentLine(string text) => AppendCopyContent(text).AppendEmptyLine();
    public TelegramMarkupMessageBuilder AppendLinkLine(string link, string title) => AppendLink(link, title).AppendEmptyLine();
}
