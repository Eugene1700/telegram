using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Realization.Extensions;

namespace Telegram.Commands.Realization.Models;

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