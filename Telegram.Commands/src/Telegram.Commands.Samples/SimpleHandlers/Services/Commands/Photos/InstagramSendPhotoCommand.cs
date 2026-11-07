using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "sendphoto", Authorized = true, Swarms = new[] { "instagram" })]
public sealed class InstagramSendPhotoCommand(ISampleMessageSender sender) : SendPhotoCommand(sender), IQueryCommand<global::Instagram.Messaging.Models.InstagramMessage>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Instagram.Messaging.Models.InstagramMessage>.Execute(IQueryContext<global::Instagram.Messaging.Models.InstagramMessage> query)
        => Execute(query);
}
