using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "selectdropdownitem_callback", Authorized = true, Swarms = new[] { "vk" })]
public sealed class VkSelectDropDownItemCallback(ISampleMessageSender sender)
    : SelectDropDownItemCallback(sender), ISessionCommand<global::VkNet.Model.MessageEvent, DropDownSessionObject>
{
    Task<ICommandExecutionResult> ISessionCommand<global::VkNet.Model.MessageEvent, DropDownSessionObject>.Execute(IQueryContext<global::VkNet.Model.MessageEvent> query, DropDownSessionObject data)
        => Execute(query, data);
}
