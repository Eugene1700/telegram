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
[Command(Name = "start", Authorized = false)]
public abstract class StartCommand(ISampleMessageSender sender) : MessageCommandBase
{
    protected override async Task<ICommandExecutionResult> ExecuteCore<TQuery>(QueryContext<TQuery> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("Hello! Try /ask, /profile, /sendphoto, /dropdown, /dropdownmultiple, /myfluent or /global.")); return CommandExecutionResult.Break();
    }
}
