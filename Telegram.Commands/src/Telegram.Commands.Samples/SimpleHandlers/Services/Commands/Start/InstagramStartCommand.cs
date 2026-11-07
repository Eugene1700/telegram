using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "start", Authorized = false, Swarms = new[] { "instagram" })]
public sealed class InstagramStartCommand(ISampleMessageSender sender) : StartCommand(sender), IQueryCommand<global::Instagram.Messaging.Models.InstagramMessage>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Instagram.Messaging.Models.InstagramMessage>.Execute(IQueryContext<global::Instagram.Messaging.Models.InstagramMessage> query)
        => Execute(query);
}
