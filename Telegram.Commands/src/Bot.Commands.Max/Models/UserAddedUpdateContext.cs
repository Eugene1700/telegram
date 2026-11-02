using global::Max.Bot.Types;
using global::Max.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Max.Models;

public class UserAddedUpdateContext(UserAddedUpdate query, long? chatId = null) : QueryContext<UserAddedUpdate>(query)
{
    public override long GetChatId()
    {
        return Query.Chat?.ChatId ?? chatId ?? throw new InvalidOperationException("User-added update has no chat ID");
    }

    public override long GetFromId()
    {
        return Query.User.Id;
    }

    public override string GetData()
    {
        return string.Empty;
    }

    public override bool IsGroupMessage()
    {
        return (Query.Chat?.Type ?? ChatType.Chat).IsGroupMessage();
    }

    public override bool IsEvent()
    {
        return true;
    }

    public override EventType? GetEventType()
    {
        return EventType.ChatMemberAdded;
    }

    public override ChatArea GetChatType()
    {
        return (Query.Chat?.Type ?? ChatType.Chat).ToChatArea();
    }

    public override string? GetMessageId()
    {
        return null;
    }
}
