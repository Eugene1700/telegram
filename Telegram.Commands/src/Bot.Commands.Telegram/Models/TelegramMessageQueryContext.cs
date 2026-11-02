using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core;
using Bot.Commands.Telegram.Extensions;

namespace Bot.Commands.Telegram.Models;

public class TelegramMessageQueryContext(Message query) : QueryContext<Message>(query)
{
    public override long GetChatId()
    {
        return Query.Chat.Id;
    }

    public override long GetFromId()
    {
        return Query.From.Id;
    }

    public override string? GetData()
    {
        return Query.Type == MessageType.Text ? Query.Text : null;
    }

    public override bool IsGroupMessage()
    {
        return Query.Chat.Type.IsGroupMessage();
    }

    public override bool IsEvent()
    {
        return Query.Type switch
        {
            MessageType.NewChatMembers => true,
            _ => false
        };
    }

    public override EventType? GetEventType()
    {
        return Query.Type switch
        {
            MessageType.NewChatMembers => EventType.ChatMemberAdded,
            _ => null
        };
    }

    public override ChatArea GetChatType()
    {
        return Query.Chat.Type.ToChatArea();
    }

    public override string? GetMessageId()
    {
        return Query.MessageId.ToString();
    }
}
