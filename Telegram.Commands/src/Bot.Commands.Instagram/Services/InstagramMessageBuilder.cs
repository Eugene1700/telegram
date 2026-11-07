using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
namespace Bot.Commands.Instagram.Services;

public sealed class InstagramMessageBuilder : MessageBuilderBase
{
    protected override IMessageTextFormat Format => null!;
    protected override string PrepareText(string text) => text ?? string.Empty;
    public new InstagramMessageBuilder AppendPlain(string text) { base.AppendPlain(text); return this; }
    public new InstagramMessageBuilder AppendPlainLine(string text) { base.AppendPlainLine(text); return this; }
    public new InstagramMessageBuilder AppendPlainSpace(string text) { base.AppendPlainSpace(text); return this; }
    public new InstagramMessageBuilder AppendEmptyLine() { base.AppendEmptyLine(); return this; }
}
