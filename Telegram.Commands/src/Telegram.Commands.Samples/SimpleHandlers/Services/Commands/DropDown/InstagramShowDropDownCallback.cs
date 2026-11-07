using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "showdropdown_callback", Authorized = true, Swarms = new[] { "instagram" })]
public sealed class InstagramShowDropDownCallback(ISampleMessageSender sender)
    : ShowDropDownCallback(sender), ISessionCommand<global::Instagram.Messaging.Models.InstagramPostback, DropDownSessionObject>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Instagram.Messaging.Models.InstagramPostback, DropDownSessionObject>.Execute(IQueryContext<global::Instagram.Messaging.Models.InstagramPostback> query, DropDownSessionObject data)
        => Execute(query, data);
}
