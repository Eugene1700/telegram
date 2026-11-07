using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Vk.Services;

/// <summary>Builds plain-text VK messages, preserving common attachments and callback descriptors.</summary>
public class VkMessageBuilder : MessageBuilderBase
{
    protected override IMessageTextFormat Format => null!;
    protected override string PrepareText(string text) => text ?? string.Empty;
    public new VkMessageBuilder AppendPlain(string text) { base.AppendPlain(text); return this; }
    public new VkMessageBuilder AppendPlainLine(string text) { base.AppendPlainLine(text); return this; }
    public new VkMessageBuilder AppendPlainSpace(string text) { base.AppendPlainSpace(text); return this; }
    public new VkMessageBuilder AppendEmptyLine() { base.AppendEmptyLine(); return this; }
}
