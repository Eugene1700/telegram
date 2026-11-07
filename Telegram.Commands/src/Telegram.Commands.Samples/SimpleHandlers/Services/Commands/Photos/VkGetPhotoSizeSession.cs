using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "getphotosize_session", Authorized = true, Swarms = new[] { "vk" })]
public sealed class VkGetPhotoSizeSession(ISampleMessageSender sender)
    : GetPhotoSizeSession(sender), ISessionCommand<global::VkNet.Model.Message, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::VkNet.Model.Message, List<PhotoSessionObject>>.Execute(
        IQueryContext<global::VkNet.Model.Message> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
