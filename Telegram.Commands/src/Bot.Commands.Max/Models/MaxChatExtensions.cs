using global::Max.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;

namespace Bot.Commands.Max.Models;

public static class MaxChatExtensions
{
    public static ChatType ParseChatType(string? type) => type switch
    {
        "dialog" => ChatType.Dialog,
        "chat" => ChatType.Chat,
        "channel" => ChatType.Channel,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown Max chat type")
    };

    public static ChatArea ToChatArea(this ChatType type) => type switch
    {
        ChatType.Dialog => ChatArea.Private,
        ChatType.Chat => ChatArea.Group,
        ChatType.Channel => ChatArea.Channel,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static bool IsGroupMessage(this ChatType type) => type == ChatType.Chat;
}
