using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "name_reply", Authorized = false, Swarms = new[] { "vk" })]
public sealed class VkNameReplyCommand(ISampleMessageSender sender) : NameReplyCommand(sender), ISessionCommand<global::VkNet.Model.Message, NameReplyData>
{
    Task<ICommandExecutionResult> ISessionCommand<global::VkNet.Model.Message, NameReplyData>.Execute(IQueryContext<global::VkNet.Model.Message> query, NameReplyData data)
        => Execute(query, data);
}
