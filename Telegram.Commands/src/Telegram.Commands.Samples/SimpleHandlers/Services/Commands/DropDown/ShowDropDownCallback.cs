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
[Command(Name = "showdropdown_callback", Authorized = true)]
public abstract class ShowDropDownCallback(ISampleMessageSender sender)
{
    public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryContext<TQuery> query, DropDownSessionObject data)
    {
        data.DropDown.Show = !data.DropDown.Show; await sender.Edit(query.GetChatId(), query.GetMessageId(), SampleMenus.Message(data)); return SampleCommandResult.Next<DropDownBehavior>(data);
    }
}
