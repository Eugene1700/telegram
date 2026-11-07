using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "myfluent", Authorized = true, Swarms = new[] { "vk" })]
public sealed class VkMyFluentCommandFluent(ISampleMessageSender sender) : MyFluentCommandFluent<global::VkNet.Model.Message, global::VkNet.Model.MessageEvent>(sender) { }
