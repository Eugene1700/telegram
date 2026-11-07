using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "cancel_callback", Authorized = true, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppCancelCallback(ISampleMessageSender sender)
    : CancelCallback(sender), IQueryCommand<global::WhatsApp.Messaging.Models.WhatsAppReply>
{
    Task<ICommandExecutionResult> IQueryCommand<global::WhatsApp.Messaging.Models.WhatsAppReply>.Execute(IQueryContext<global::WhatsApp.Messaging.Models.WhatsAppReply> query)
        => Execute(query);
}
