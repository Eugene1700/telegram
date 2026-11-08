using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeaddress", Authorized = false, Swarms = new[] { "vk" })]
public sealed class VkAddressFlow(ISampleMessageSender sender) : AddressFlow<VkNet.Model.Message>(sender) { }
