using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routecity", Authorized = false, Swarms = new[] { "telegram" })]
public sealed class TelegramCityFlow(ISampleMessageSender sender) : CityFlow<global::Telegram.Bot.Types.Message>(sender) { }
