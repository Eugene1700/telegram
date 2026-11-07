using VkNet.Model;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using Bot.Commands.Vk.Models;

namespace Bot.Commands.Vk;

/// <summary>Builds native VK inline keyboards using the shared row and metadata builder.</summary>
public class VkInlineMarkupQueryBuilder
    : AdapterInlineMarkupQueryBuilder<VkInlineMarkupQueryBuilder, MessageEvent, MessageKeyboardButton, MessageKeyboard>
{
    protected override MessageKeyboardButton ToButton(CallbackDataWithCommand callback) => callback.ToButton();
    protected override MessageKeyboard ToMarkup(MessageKeyboardButton[][] rows)
        => new() { Inline = true, OneTime = false, Buttons = rows };
}
