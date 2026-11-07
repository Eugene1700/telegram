using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "getphotosize_session", Authorized = true, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppGetPhotoSizeSession(ISampleMessageSender sender)
    : GetPhotoSizeSession(sender), ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppMessage, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppMessage, List<PhotoSessionObject>>.Execute(
        IQueryContext<global::WhatsApp.Messaging.Models.WhatsAppMessage> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
