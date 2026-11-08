using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeorder", Authorized = false, Swarms = new[] { "telegram" })]
public sealed class TelegramOrderFlow(ISampleMessageSender sender) : OrderFlow<global::Telegram.Bot.Types.Message>(sender) { }
