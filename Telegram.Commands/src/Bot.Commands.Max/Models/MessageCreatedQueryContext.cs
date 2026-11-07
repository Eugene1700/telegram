using global::Max.Bot.Types;
using global::Max.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Max.Models;

public class MessageCreatedQueryContext(Chat? chat, Message query) : IQueryContext<Message>
{
    public Message Query { get; } = query;

    public long GetChatId()
    {
        return chat?.ChatId ?? Query.Recipient?.ChatId ?? throw new InvalidOperationException("Message has no chat ID");
    }

    public long GetFromId()
    {
        return Query.Sender.Id;
    }

    public string GetData()
    {
        return Query.Text;
    }

    public bool IsGroupMessage()
    {
        return (chat?.Type ?? Query.Recipient?.ChatType ?? throw new InvalidOperationException("Message has no chat type")).IsGroupMessage();
    }

    public bool IsEvent()
    {
        return false;
    }

    public EventType? GetEventType()
    {
        return null;
    }

    public ChatArea GetChatType()
    {
        return (chat?.Type ?? Query.Recipient?.ChatType ?? throw new InvalidOperationException("Message has no chat type")).ToChatArea();
    }

    public string? GetMessageId()
    {
        return Query.Mid;
    }
}
