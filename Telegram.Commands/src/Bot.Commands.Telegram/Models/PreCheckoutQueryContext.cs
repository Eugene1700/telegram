using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.Payments;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core;
using Bot.Commands.Telegram.Extensions;

namespace Bot.Commands.Telegram.Models;

public class PreCheckoutQueryContext(PreCheckoutQuery query) : QueryContext<PreCheckoutQuery>(query)
{
    public override long GetChatId()
    {
        return Query.From.Id;
    }

    public override long GetFromId()
    {
        return Query.From.Id;
    }

    public override string GetData()
    {
        return Query.InvoicePayload;
    }

    public override bool IsGroupMessage()
    {
        return ChatType.Private.IsGroupMessage();
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
        return ChatArea.Private;
    }

    public override string? GetMessageId()
    {
        return null;
    }
}