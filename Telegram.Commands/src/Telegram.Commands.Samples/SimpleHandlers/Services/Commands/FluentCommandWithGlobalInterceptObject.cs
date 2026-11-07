using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{

    public class FluentCommandWithGlobalInterceptObject
    {
        public bool IsIntercept { get; set; }
        public long ChatId { get; set; }
    }
}
