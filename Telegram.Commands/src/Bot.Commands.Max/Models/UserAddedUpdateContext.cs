using global::Max.Bot.Types;
using global::Max.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Max.Models;

public class UserAddedUpdateContext(UserAddedUpdate query, long? chatId = null) : IQueryContext<UserAddedUpdate>
{
    public UserAddedUpdate Query { get; } = query;

    public long GetChatId()
    {
        return Query.Chat?.ChatId ?? chatId ?? throw new InvalidOperationException("User-added update has no chat ID");
    }

    public long GetFromId()
    {
        return Query.User.Id;
    }

    public string GetData()
    {
        return string.Empty;
    }

    public bool IsGroupMessage()
    {
        return (Query.Chat?.Type ?? ChatType.Chat).IsGroupMessage();
    }

    public bool IsEvent()
    {
        return true;
    }

    public EventType? GetEventType()
    {
        return EventType.ChatMemberAdded;
    }

    public ChatArea GetChatType()
    {
        return (Query.Chat?.Type ?? ChatType.Chat).ToChatArea();
    }

    public string? GetMessageId()
    {
        return null;
    }
}
