using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "otherfluent", Authorized = true, Swarms = new[] { "telegram" })]
public sealed class TelegramMyFluentInitCommand(ISampleMessageSender sender) : MyFluentInitCommand<global::Telegram.Bot.Types.Message, global::Telegram.Bot.Types.CallbackQuery>(sender) { }
