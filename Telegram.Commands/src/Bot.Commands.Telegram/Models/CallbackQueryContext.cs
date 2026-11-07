using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core;
using Bot.Commands.Telegram.Extensions;

namespace Bot.Commands.Telegram.Models;

public class CallbackQueryContext(CallbackQuery query) : IQueryContext<CallbackQuery>
{
    public CallbackQuery Query { get; } = query;

    public long GetChatId()
    {
        return Query.Message.Chat.Id;
    }

    public long GetFromId()
    {
        return Query.From.Id;
    }

    public string? GetData()
    {
        return Query.Data;
    }

    public bool IsGroupMessage()
    {
        return Query.Message.Chat.Type.IsGroupMessage();
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
        return Query.Message.Chat.Type.ToChatArea();
    }

    public string? GetMessageId()
    {
        return Query.Message?.MessageId.ToString();
    }
}