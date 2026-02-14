using Max.Bot.Types;
using Max.Bot.Types.Enums;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;

namespace Bot.Command.Max.Models;

public class MessageCreatedQueryContext(Chat chat, Message query) : QueryContext<Message>(query)
{
    public override long GetChatId()
    {
        return chat.ChatId;
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
        return chat.Type == ChatType.Channel;
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
        //todo
        return chat.Type;
    }

    public override string? GetMessageId()
    {
        return Query.Mid;
    }
}