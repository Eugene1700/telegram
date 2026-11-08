using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routecity", Authorized = false, Swarms = new[] { "instagram" })]
public sealed class InstagramCityFlow(ISampleMessageSender sender) : CityFlow<global::Instagram.Messaging.Models.InstagramMessage>(sender) { }
