using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
namespace Bot.Commands.WhatsApp.Services;

public sealed class WhatsAppMessageBuilder : MessageBuilderBase
{
    protected override IMessageTextFormat Format => null!;
    protected override string PrepareText(string text) => text ?? string.Empty;
    public new WhatsAppMessageBuilder AppendPlain(string text) { base.AppendPlain(text); return this; }
    public new WhatsAppMessageBuilder AppendPlainLine(string text) { base.AppendPlainLine(text); return this; }
    public new WhatsAppMessageBuilder AppendPlainSpace(string text) { base.AppendPlainSpace(text); return this; }
    public new WhatsAppMessageBuilder AppendEmptyLine() { base.AppendEmptyLine(); return this; }
}
