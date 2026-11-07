using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "getphotosize_session", Authorized = true, Swarms = new[] { "instagram" })]
public sealed class InstagramGetPhotoSizeSession(ISampleMessageSender sender)
    : GetPhotoSizeSession(sender), ISessionCommand<global::Instagram.Messaging.Models.InstagramMessage, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Instagram.Messaging.Models.InstagramMessage, List<PhotoSessionObject>>.Execute(
        IQueryContext<global::Instagram.Messaging.Models.InstagramMessage> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
