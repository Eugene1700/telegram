using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.UI.DropDown;
using SimpleHandlers.Services.Commands.Models;
namespace SimpleHandlers.Services.Commands;
[Command(Name = "dropdown", Authorized = true)]
public abstract class DropDownMenuCommand(ISampleMessageSender sender)
{
    public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryContext<TQuery> query)
    {
        var data = SampleMenus.DropDown(false); await sender.Send(query.GetChatId(), SampleMenus.Message(data)); return SampleCommandResult.Next<DropDownBehavior>(data);
    }
}
