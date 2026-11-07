using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "showresult_callback", Authorized = true, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppShowResultCallback(ISampleMessageSender sender)
    : ShowResultCallback(sender), ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppReply, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppReply, List<PhotoSessionObject>>.Execute(IQueryContext<global::WhatsApp.Messaging.Models.WhatsAppReply> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
