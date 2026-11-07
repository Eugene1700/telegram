using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent", Authorized = true, Swarms = new[] { "instagram" })]
public sealed class InstagramMyFluentCommandFluent(ISampleMessageSender sender) : MyFluentCommandFluent<global::Instagram.Messaging.Models.InstagramMessage, global::Instagram.Messaging.Models.InstagramPostback>(sender) { }
