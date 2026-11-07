using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "ask", Authorized = false, Swarms = new[] { "telegram" })]
public sealed class TelegramAskCommand(ISampleMessageSender sender) : AskCommand(sender) { }
