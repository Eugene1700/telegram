using global::Max.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Max.Models;

public class MessageCallbackContext(Chat? chat, CallbackQuery query, Message? message = null) : IQueryContext<CallbackQuery>
{
    public CallbackQuery Query { get; } = query;

    private Message Message => message ?? Query.Message ?? throw new InvalidOperationException("Callback has no message");
    public long GetChatId()
    {
        return chat?.ChatId ?? Message.Recipient?.ChatId ?? throw new InvalidOperationException("Callback has no chat ID");
    }

    public long GetFromId()
    {
        return Query.User.Id;
    }

    public string GetData()
    {
        return Query.Payload;
    }

    public bool IsGroupMessage()
    {
        return (chat?.Type ?? Message.Recipient?.ChatType ?? throw new InvalidOperationException("Callback message has no chat type")).IsGroupMessage();
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
        return (chat?.Type ?? Message.Recipient?.ChatType ?? throw new InvalidOperationException("Callback message has no chat type")).ToChatArea();
    }

    public string? GetMessageId()
    {
        return Message.Mid;
    }
}
