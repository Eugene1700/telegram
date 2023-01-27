using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent")]
public class MyFluentCommand : IQueryTelegramCommand<Message>
{
    public async Task<ITelegramCommandExecutionResult> Execute(Message query)
    {
        return TelegramCommandExecutionResult.AheadFluent<MyFluentCommandFluent, MyObject>(new MyObject(), null);
    }
}