using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "selectdropdownitem_callback", Authorized = true, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppSelectDropDownItemCallback(ISampleMessageSender sender)
    : SelectDropDownItemCallback(sender), ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppReply, DropDownSessionObject>
{
    Task<ICommandExecutionResult> ISessionCommand<global::WhatsApp.Messaging.Models.WhatsAppReply, DropDownSessionObject>.Execute(IQueryContext<global::WhatsApp.Messaging.Models.WhatsAppReply> query, DropDownSessionObject data)
        => Execute(query, data);
}
