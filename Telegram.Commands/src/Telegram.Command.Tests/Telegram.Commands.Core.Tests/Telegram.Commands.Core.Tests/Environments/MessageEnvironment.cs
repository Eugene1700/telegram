using Telegram.Bot.Types;

namespace Telegram.Commands.Core.Tests.Environments;

public class MessageEnvironment
{
    public Message CreateMessage()
    {
        return new Message
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
    }
}