using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "ask", Authorized = false, Swarms = new[] { "vk" })]
public sealed class VkAskCommand(ISampleMessageSender sender) : AskCommand(sender), IQueryCommand<global::VkNet.Model.Message>
{
    Task<ICommandExecutionResult> IQueryCommand<global::VkNet.Model.Message>.Execute(IQueryContext<global::VkNet.Model.Message> query)
        => Execute(query);
}
