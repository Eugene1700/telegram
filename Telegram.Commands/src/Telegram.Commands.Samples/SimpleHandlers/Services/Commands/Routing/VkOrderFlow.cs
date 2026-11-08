using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeorder", Authorized = false, Swarms = new[] { "vk" })]
public sealed class VkOrderFlow(ISampleMessageSender sender) : OrderFlow<VkNet.Model.Message>(sender) { }
