using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "name_reply", Authorized = false, Swarms = new[] { "instagram" })]
public sealed class InstagramNameReplyCommand(ISampleMessageSender sender) : NameReplyCommand(sender), ISessionCommand<global::Instagram.Messaging.Models.InstagramMessage, NameReplyData>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Instagram.Messaging.Models.InstagramMessage, NameReplyData>.Execute(IQueryContext<global::Instagram.Messaging.Models.InstagramMessage> query, NameReplyData data)
        => Execute(query, data);
}
