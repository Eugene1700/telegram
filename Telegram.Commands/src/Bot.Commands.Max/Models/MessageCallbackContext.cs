using Max.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;

namespace Bot.Command.Max.Models;

public class MessageCallbackContext(Chat chat, CallbackQuery query) : QueryContext<CallbackQuery>(query)
{
    public override long GetChatId()
    {
        return chat.ChatId;
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public override string? GetMessageId()
    {
        return Query.Message.Mid;
    }
}