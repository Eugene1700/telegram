using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent", Authorized = true, Platform = "max")]
public sealed class MaxMyFluentCommandFluent(ISampleMessageSender sender) : MyFluentCommandFluent<global::Max.Bot.Types.Message, global::Max.Bot.Types.CallbackQuery>(sender) { }
