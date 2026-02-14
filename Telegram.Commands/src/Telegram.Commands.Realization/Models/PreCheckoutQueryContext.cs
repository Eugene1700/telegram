using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Realization.Extensions;

namespace Telegram.Commands.Realization.Models;

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