using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent", Authorized = true, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppMyFluentCommandFluent(ISampleMessageSender sender) : MyFluentCommandFluent<global::WhatsApp.Messaging.Models.WhatsAppMessage, global::WhatsApp.Messaging.Models.WhatsAppReply>(sender) { }
