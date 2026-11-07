using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "cancel_callback", Authorized = true, Swarms = new[] { "vk" })]
public sealed class VkCancelCallback(ISampleMessageSender sender)
    : CancelCallback(sender), IQueryCommand<global::VkNet.Model.MessageEvent>
{
    Task<ICommandExecutionResult> IQueryCommand<global::VkNet.Model.MessageEvent>.Execute(IQueryContext<global::VkNet.Model.MessageEvent> query)
        => Execute(query);
}
