using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "showresult_callback", Authorized = true, Swarms = new[] { "vk" })]
public sealed class VkShowResultCallback(ISampleMessageSender sender)
    : ShowResultCallback(sender), ISessionCommand<global::VkNet.Model.MessageEvent, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::VkNet.Model.MessageEvent, List<PhotoSessionObject>>.Execute(IQueryContext<global::VkNet.Model.MessageEvent> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
