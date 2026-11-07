using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;
using Instagram.Messaging.Models;
namespace Bot.Commands.Instagram.Models;

public static class InstagramMessageExtensions
{
    public static IReadOnlyList<InstagramButton> ToButtons(this IBotMessage message) => Build(message.Callbacks);
    public static IReadOnlyList<InstagramButton> GetResult(this InlineMarkupQueryBuilder builder) => Build(builder.GetCallbacks());
    private static IReadOnlyList<InstagramButton> Build(IEnumerable<CallbackDataWithCommand>? callbacks)
    {
        var builder = new InstagramInlineMarkupQueryBuilder();
        foreach (var row in (callbacks ?? []).GroupBy(c => c.Row))
            builder.InlineKeyboardButtonsRow(row.ToArray());
        return builder.GetResult();
    }
    public static string ToInstagramText(this IBotMessage message)
    {
        if (message.Format != null) throw new NotSupportedException("Instagram supports plain text; messenger-specific formatting is not supported.");
        return message.Text;
    }
}
