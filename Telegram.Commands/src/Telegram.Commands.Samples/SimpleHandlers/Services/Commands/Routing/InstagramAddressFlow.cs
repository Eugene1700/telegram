using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeaddress", Authorized = false, Swarms = new[] { "instagram" })]
public sealed class InstagramAddressFlow(ISampleMessageSender sender) : AddressFlow<global::Instagram.Messaging.Models.InstagramMessage>(sender) { }
