using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Realization.Extensions;

namespace Telegram.Commands.Realization.Models;

public class CallbackQueryContext(CallbackQuery query) : QueryContext<CallbackQuery>(query)
{
    public override long GetChatId()
    {
        return Query.Message.Chat.Id;
    }

    public override long GetFromId()
    {
        return Query.From.Id;
    }

    public override string? GetData()
    {
        return Query.Data;
    }

    public override bool IsGroupMessage()
    {
        return Query.Message.Chat.Type.IsGroupMessage();
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
        return Query.Message.Chat.Type.ToChatArea();
    }

    public override string? GetMessageId()
    {
        return Query.Message?.MessageId.ToString();
    }
}