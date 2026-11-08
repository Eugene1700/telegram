using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeaddress", Authorized = false, Swarms = new[] { "max" })]
public sealed class MaxAddressFlow(ISampleMessageSender sender) : AddressFlow<global::Max.Bot.Types.Message>(sender) { }
