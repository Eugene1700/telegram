using global::Max.Bot.Types;
using global::Max.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Max.Models;

public class MessageCreatedQueryContext(Chat? chat, Message query) : QueryContext<Message>(query)
{
    public override long GetChatId()
    {
        return chat?.ChatId ?? Query.Recipient?.ChatId ?? throw new InvalidOperationException("Message has no chat ID");
    }

    public override long GetFromId()
    {
        return Query.Sender.Id;
    }

    public override string GetData()
    {
        return Query.Text;
    }

    public override bool IsGroupMessage()
    {
        return (chat?.Type ?? Query.Recipient?.ChatType ?? throw new InvalidOperationException("Message has no chat type")).IsGroupMessage();
    }

    public override bool IsEvent()
    {
        return false;
    }

    public override EventType? GetEventType()
    {
        return null;
    }

    public override ChatArea GetChatType()
    {
        return (chat?.Type ?? Query.Recipient?.ChatType ?? throw new InvalidOperationException("Message has no chat type")).ToChatArea();
    }

    public override string? GetMessageId()
    {
        return Query.Mid;
    }
}
