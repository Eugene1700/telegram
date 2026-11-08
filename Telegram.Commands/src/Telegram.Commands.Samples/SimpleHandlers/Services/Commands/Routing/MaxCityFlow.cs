using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routecity", Authorized = false, Swarms = new[] { "max" })]
public sealed class MaxCityFlow(ISampleMessageSender sender) : CityFlow<global::Max.Bot.Types.Message>(sender) { }
