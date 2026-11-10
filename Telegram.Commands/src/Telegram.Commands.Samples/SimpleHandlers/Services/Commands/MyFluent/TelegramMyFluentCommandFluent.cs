using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent", Authorized = true, Platform = "telegram")]
public sealed class TelegramMyFluentCommandFluent(ISampleMessageSender sender) : MyFluentCommandFluent<global::Telegram.Bot.Types.Message, global::Telegram.Bot.Types.CallbackQuery>(sender) { }
