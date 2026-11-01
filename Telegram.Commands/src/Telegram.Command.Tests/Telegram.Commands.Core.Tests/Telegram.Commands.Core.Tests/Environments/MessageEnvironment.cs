using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Realization.Models;

namespace Telegram.Commands.Core.Tests.Environments;

public class MessageEnvironment
{
    public Message CreateMessage(Action<Message> updater = null)
    {
        var message =  new Message
        {
            Text = "My Text",
            Chat =new Chat
            {
                Id = 123456789
            },
            From = new User
            {
                Id = 987654321
            }
        };
        
        updater?.Invoke(message);
        return message;
    }

    public QueryContext<Message> CreateMessageQueryContext(Action<Message> updater = null)
    {
        var message = CreateMessage(updater);
        return new TelegramMessageQueryContext(message);
    }
}