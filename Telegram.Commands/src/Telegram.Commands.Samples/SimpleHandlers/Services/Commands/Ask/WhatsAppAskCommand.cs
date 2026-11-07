using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "ask", Authorized = false, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppAskCommand(ISampleMessageSender sender) : AskCommand(sender), IQueryCommand<global::WhatsApp.Messaging.Models.WhatsAppMessage>
{
    Task<ICommandExecutionResult> IQueryCommand<global::WhatsApp.Messaging.Models.WhatsAppMessage>.Execute(IQueryContext<global::WhatsApp.Messaging.Models.WhatsAppMessage> query)
        => Execute(query);
}
