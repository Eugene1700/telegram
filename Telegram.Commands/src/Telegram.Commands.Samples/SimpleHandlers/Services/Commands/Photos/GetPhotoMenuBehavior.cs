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
[Command(Name = "getphotomenu_behavior")]
public abstract class GetPhotoMenuBehavior : IBehaviorCommand<List<PhotoSessionObject>>
{
    public Task<ICommandExecutionResult> DefaultExecute<TQuery>(IQueryContext<TQuery> query, List<PhotoSessionObject> data)
        => throw new BotDomainException("Please choose an action from the menu.");
    public Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> currentCommand, IQueryContext<TQuery> query, List<PhotoSessionObject> data)
        => currentCommand is CancelCallback ? currentCommand.Execute(query) : DefaultExecute(query, data);
    public Task<ICommandExecutionResult> Execute<TQuery>(ISessionCommand<TQuery, List<PhotoSessionObject>> currentCommand, IQueryContext<TQuery> query, List<PhotoSessionObject> data)
        => currentCommand is ShowResultCallback or NextPhotoCallback ? currentCommand.Execute(query, data) : DefaultExecute(query, data);
}
