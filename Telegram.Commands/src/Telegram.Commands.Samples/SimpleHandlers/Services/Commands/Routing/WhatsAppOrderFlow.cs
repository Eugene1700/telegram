using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeorder", Authorized = false, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppOrderFlow(ISampleMessageSender sender) : OrderFlow<global::WhatsApp.Messaging.Models.WhatsAppMessage>(sender) { }
