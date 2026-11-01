using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;

namespace Bot.Command.Max.Models;

public class UserAddedUpdateContext(UserAddedUpdate query) : QueryContext<UserAddedUpdate>(query)
{
    public override long GetChatId()
    {
        return Query.Chat.ChatId;
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
        return Query.Chat.Type == ChatType.Channel;
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
        throw new NotImplementedException();
    }

    public override string? GetMessageId()
    {
        return null;
    }
}