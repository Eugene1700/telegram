using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeorder", Authorized = false, Swarms = new[] { "max" })]
public sealed class MaxOrderFlow(ISampleMessageSender sender) : OrderFlow<global::Max.Bot.Types.Message>(sender) { }
