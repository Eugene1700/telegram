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
[Command(Name = "nextphoto_callback", Authorized = true)]
public abstract class NextPhotoCallback(ISampleMessageSender sender)
{
    public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryContext<TQuery> query, List<PhotoSessionObject> data)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("Ok, send me photo")); return SampleCommandResult.Next<GetPhotoSizeSession>(data, null);
    }
}
