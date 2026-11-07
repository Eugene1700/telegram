using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands;
[Command(Name = "cancel_callback", Authorized = true)]
public abstract class CancelCallback(ISampleMessageSender sender)
{
    public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryContext<TQuery> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("Cancelled. Try /sendphoto or /dropdown.")); return CommandExecutionResult.Break();
    }
}
