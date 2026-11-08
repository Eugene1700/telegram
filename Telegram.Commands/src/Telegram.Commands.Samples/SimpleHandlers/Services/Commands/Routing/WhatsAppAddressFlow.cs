using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeaddress", Authorized = false, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppAddressFlow(ISampleMessageSender sender) : AddressFlow<global::WhatsApp.Messaging.Models.WhatsAppMessage>(sender) { }
