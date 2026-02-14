using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Realization.Extensions;

namespace Telegram.Commands.Realization.Models;

public class ChatMemberUpdateQueryContext: QueryContext<ChatMemberUpdated>
{
    public ChatMemberUpdateQueryContext(ChatMemberUpdated query) : base(query)
    {
    }

    public override long GetChatId()
    {
        return Query.Chat.Id;
    }

    public override long GetFromId()
    {
        return Query.From.Id;
    }

    public override string GetData()
    {
        return null;
    }

    public override bool IsGroupMessage()
    {
        return Query.Chat.Type.IsGroupMessage();
    }

    public override bool IsEvent()
    {
        return true;
    }

    public override EventType? GetEventType()
    {
        return EventType.BotMemberAddedToChat;
    }

    public override ChatArea GetChatType()
    {
        return Query.Chat.Type.ToChatArea();
    }

    public override string? GetMessageId()
    {
        return null;
    }
}