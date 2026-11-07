using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core;
using Bot.Commands.Telegram.Extensions;

namespace Bot.Commands.Telegram.Models;

public class TelegramMessageQueryContext(Message query) : IQueryContext<Message>
{
    public Message Query { get; } = query;

    public long GetChatId()
    {
        return Query.Chat.Id;
    }

    public long GetFromId()
    {
        return Query.From.Id;
    }

    public string? GetData()
    {
        return Query.Type == MessageType.Text ? Query.Text : null;
    }

    public bool IsGroupMessage()
    {
        return Query.Chat.Type.IsGroupMessage();
    }

    public bool IsEvent()
    {
        return Query.Type switch
        {
            MessageType.NewChatMembers => true,
            _ => false
        };
    }

    public EventType? GetEventType()
    {
        return Query.Type switch
        {
            MessageType.NewChatMembers => EventType.ChatMemberAdded,
            _ => null
        };
    }

    public ChatArea GetChatType()
    {
        return Query.Chat.Type.ToChatArea();
    }

    public string? GetMessageId()
    {
        return Query.MessageId.ToString();
    }
}
