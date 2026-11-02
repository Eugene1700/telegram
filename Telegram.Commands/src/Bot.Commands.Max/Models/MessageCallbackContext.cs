using global::Max.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Max.Models;

public class MessageCallbackContext(Chat? chat, CallbackQuery query, Message? message = null) : QueryContext<CallbackQuery>(query)
{
    private Message Message => message ?? Query.Message ?? throw new InvalidOperationException("Callback has no message");
    public override long GetChatId()
    {
        return chat?.ChatId ?? Message.Recipient?.ChatId ?? throw new InvalidOperationException("Callback has no chat ID");
    }

    public override long GetFromId()
    {
        return Query.User.Id;
    }

    public override string GetData()
    {
        return Query.Payload;
    }

    public override bool IsGroupMessage()
    {
        return (chat?.Type ?? MaxChatExtensions.ParseChatType(Message.Recipient?.ChatType)).IsGroupMessage();
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
        return (chat?.Type ?? MaxChatExtensions.ParseChatType(Message.Recipient?.ChatType)).ToChatArea();
    }

    public override string? GetMessageId()
    {
        return Message.Mid;
    }
}
