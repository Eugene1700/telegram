using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Telegram.Extensions;
using Telegram.Bot.Types;

namespace Bot.Commands.Telegram.Models;

public class ChatMemberUpdateQueryContext: IQueryContext<ChatMemberUpdated>
{
    public ChatMemberUpdated Query { get; }

    public ChatMemberUpdateQueryContext(ChatMemberUpdated query)
    {
        Query = query;
    }

    public long GetChatId()
    {
        return Query.Chat.Id;
    }

    public long GetFromId()
    {
        return Query.From.Id;
    }

    public string GetData()
    {
        return null;
    }

    public bool IsGroupMessage()
    {
        return Query.Chat.Type.IsGroupMessage();
    }

    public bool IsEvent()
    {
        return true;
    }

    public EventType? GetEventType()
    {
        return EventType.BotMemberAddedToChat;
    }

    public ChatArea GetChatType()
    {
        return Query.Chat.Type.ToChatArea();
    }

    public string? GetMessageId()
    {
        return null;
    }
}