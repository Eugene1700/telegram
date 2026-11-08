using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routecity", Authorized = false, Swarms = new[] { "vk" })]
public sealed class VkCityFlow(ISampleMessageSender sender) : CityFlow<VkNet.Model.Message>(sender) { }
