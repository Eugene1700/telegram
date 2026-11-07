using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "cancel_callback", Authorized = true, Swarms = new[] { "instagram" })]
public sealed class InstagramCancelCallback(ISampleMessageSender sender)
    : CancelCallback(sender), IQueryCommand<global::Instagram.Messaging.Models.InstagramPostback>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Instagram.Messaging.Models.InstagramPostback>.Execute(IQueryContext<global::Instagram.Messaging.Models.InstagramPostback> query)
        => Execute(query);
}
