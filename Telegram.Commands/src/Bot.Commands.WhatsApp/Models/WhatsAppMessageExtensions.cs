using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using WhatsApp.Messaging.Models;
namespace Bot.Commands.WhatsApp.Models;

public static class WhatsAppMessageExtensions
{
    public static IReadOnlyList<WhatsAppReplyButton> ToButtons(this IBotMessage message) => Build(message.Callbacks);
    public static IReadOnlyList<WhatsAppReplyButton> GetResult(this InlineMarkupQueryBuilder builder) => Build(builder.GetCallbacks());
    private static IReadOnlyList<WhatsAppReplyButton> Build(IEnumerable<CallbackDataWithCommand>? callbacks)
    {
        var builder = new WhatsAppInlineMarkupQueryBuilder();
        foreach (var row in (callbacks ?? []).GroupBy(c => c.Row))
            builder.InlineKeyboardButtonsRow(row.ToArray());
        return builder.GetResult();
    }
    public static string ToWhatsAppText(this IBotMessage message)
    {
        if (message.Format != null) throw new NotSupportedException("WhatsApp supports plain text; messenger-specific formatting is not supported.");
        return message.Text;
    }
}
