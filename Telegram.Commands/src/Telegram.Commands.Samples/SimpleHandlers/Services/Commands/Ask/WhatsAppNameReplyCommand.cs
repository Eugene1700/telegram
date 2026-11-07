using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "name_reply", Authorized = false, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppNameReplyCommand(ISampleMessageSender sender) : NameReplyCommand(sender), ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppMessage, NameReplyData>
{
    Task<ICommandExecutionResult> ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppMessage, NameReplyData>.Execute(IQueryContext<global::WhatsApp.Messaging.Models.WhatsAppMessage> query, NameReplyData data)
        => Execute(query, data);
}
