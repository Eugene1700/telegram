using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.Payments;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core;
using Bot.Commands.Telegram.Extensions;

namespace Bot.Commands.Telegram.Models;

public class PreCheckoutQueryContext(PreCheckoutQuery query) : IQueryContext<PreCheckoutQuery>
{
    public PreCheckoutQuery Query { get; } = query;

    public long GetChatId()
    {
        return Query.From.Id;
    }

    public long GetFromId()
    {
        return Query.From.Id;
    }

    public string GetData()
    {
        return Query.InvoicePayload;
    }

    public bool IsGroupMessage()
    {
        return ChatType.Private.IsGroupMessage();
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
        return ChatArea.Private;
    }

    public string? GetMessageId()
    {
        return null;
    }
}