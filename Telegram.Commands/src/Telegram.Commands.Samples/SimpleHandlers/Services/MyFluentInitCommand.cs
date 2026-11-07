using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;
using SimpleHandlers.Services.Commands;
namespace SimpleHandlers.Services;
[Command(Name = "otherfluent")]
public abstract class MyFluentInitCommand<TMessage, TCallback>(ISampleMessageSender sender) : MessageCommandBase
    where TMessage : class
    where TCallback : class
{
    protected override async Task<ICommandExecutionResult> ExecuteCore<TQuery>(QueryContext<TQuery> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("Hi! Tell me your name, please"));
        return CommandExecutionResult.AheadFluent<MyFluentCommandFluent<TMessage, TCallback>, MyObject, States>(
            new MyObject { ChatId = query.GetChatId() }, false, null);
    }
}
