using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeorder", Authorized = false, Swarms = new[] { "instagram" })]
public sealed class InstagramOrderFlow(ISampleMessageSender sender) : OrderFlow<global::Instagram.Messaging.Models.InstagramMessage>(sender) { }
