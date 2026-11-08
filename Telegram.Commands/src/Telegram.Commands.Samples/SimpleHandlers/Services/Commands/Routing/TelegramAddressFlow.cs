using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeaddress", Authorized = false, Swarms = new[] { "telegram" })]
public sealed class TelegramAddressFlow(ISampleMessageSender sender) : AddressFlow<global::Telegram.Bot.Types.Message>(sender) { }
