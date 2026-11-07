using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services.Commands;

[Command(Name = "ask", Authorized = false)]
public abstract class AskCommand(ISampleMessageSender sender)
{
    public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryContext<TQuery> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("What should I call you?"));
        return SampleCommandResult.Next<NameReplyCommand>(new NameReplyData());
    }
}
