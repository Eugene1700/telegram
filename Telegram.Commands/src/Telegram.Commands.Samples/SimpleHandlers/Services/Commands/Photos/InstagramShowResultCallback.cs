using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "showresult_callback", Authorized = true, Swarms = new[] { "instagram" })]
public sealed class InstagramShowResultCallback(ISampleMessageSender sender)
    : ShowResultCallback(sender), ISessionCommand<global::Instagram.Messaging.Models.InstagramPostback, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Instagram.Messaging.Models.InstagramPostback, List<PhotoSessionObject>>.Execute(IQueryContext<global::Instagram.Messaging.Models.InstagramPostback> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
