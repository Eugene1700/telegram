using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "otherfluent", Authorized = true, Swarms = new[] { "instagram" })]
public sealed class InstagramMyFluentInitCommand(ISampleMessageSender sender) : MyFluentInitCommand<global::Instagram.Messaging.Models.InstagramMessage, global::Instagram.Messaging.Models.InstagramPostback>(sender), IQueryCommand<global::Instagram.Messaging.Models.InstagramMessage>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Instagram.Messaging.Models.InstagramMessage>.Execute(IQueryContext<global::Instagram.Messaging.Models.InstagramMessage> query)
        => Execute(query);
}
