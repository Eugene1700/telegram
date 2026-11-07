using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Max.Services;

/// <summary>Builds messages using Max Markdown syntax.</summary>
public class MaxMarkupMessageBuilder : MessageBuilderBase
{
    protected override IMessageTextFormat Format => new MaxTextFormat(global::Max.Bot.Types.Enums.TextFormat.Markdown);
    protected override string PrepareText(string text) => System.Text.RegularExpressions.Regex.Replace(text ?? string.Empty, @"([\\\\`*_{}\[\]()#+\-.!>~|^=])", @"\$1");

    /// <summary>Appends trusted, already formatted Markdown without escaping.</summary>
    public MaxMarkupMessageBuilder AppendMarkdown(string text) { Append(text, false); return this; }
    public new MaxMarkupMessageBuilder AppendPlain(string text) { base.AppendPlain(text); return this; }
    public new MaxMarkupMessageBuilder AppendPlainLine(string text) { base.AppendPlainLine(text); return this; }
    public new MaxMarkupMessageBuilder AppendPlainSpace(string text) { base.AppendPlainSpace(text); return this; }
    public new MaxMarkupMessageBuilder AppendEmptyLine() { base.AppendEmptyLine(); return this; }

    public MaxMarkupMessageBuilder AppendBold(string text) => AppendMarkdown("**" + PrepareText(text) + "**");
    public MaxMarkupMessageBuilder AppendUnderline(string text) => AppendMarkdown("++" + PrepareText(text) + "++");
    public MaxMarkupMessageBuilder AppendCopyContent(string text) => AppendMarkdown("`" + (text ?? string.Empty).Replace(@"\", @"\\").Replace("`", @"\`") + "`");
    public MaxMarkupMessageBuilder AppendLink(string link, string title) => AppendMarkdown("[" + PrepareText(title) + "](" + link.Replace(@"\", @"\\").Replace(")", @"\)") + ")");
    public MaxMarkupMessageBuilder AppendBoldLine(string text) => AppendBold(text).AppendEmptyLine();
    public MaxMarkupMessageBuilder AppendUnderlineLine(string text) => AppendUnderline(text).AppendEmptyLine();
    public MaxMarkupMessageBuilder AppendCopyContentLine(string text) => AppendCopyContent(text).AppendEmptyLine();
    public MaxMarkupMessageBuilder AppendLinkLine(string link, string title) => AppendLink(link, title).AppendEmptyLine();
}
