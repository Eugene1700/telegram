using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routecity", Authorized = false, Swarms = new[] { "whatsapp" })]
public sealed class WhatsAppCityFlow(ISampleMessageSender sender) : CityFlow<global::WhatsApp.Messaging.Models.WhatsAppMessage>(sender) { }
