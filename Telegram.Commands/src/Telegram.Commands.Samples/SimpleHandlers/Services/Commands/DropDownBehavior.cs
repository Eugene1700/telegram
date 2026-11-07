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
[Command(Name = "dropdown_behavior")]
public abstract class DropDownBehavior : IBehaviorCommand<DropDownSessionObject>
{
    public Task<ICommandExecutionResult> DefaultExecute<TQuery>(QueryContext<TQuery> query, DropDownSessionObject data)
        => throw new BotDomainException("Please choose an action from the menu.");
    public Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> currentCommand, QueryContext<TQuery> query, DropDownSessionObject data)
        => currentCommand is CancelCallback ? currentCommand.Execute(query) : DefaultExecute(query, data);
    public Task<ICommandExecutionResult> Execute<TQuery>(ISessionCommand<TQuery, DropDownSessionObject> currentCommand, QueryContext<TQuery> query, DropDownSessionObject data)
        => currentCommand is ShowDropDownCallback or SelectDropDownItemCallback ? currentCommand.Execute(query, data) : DefaultExecute(query, data);
}
