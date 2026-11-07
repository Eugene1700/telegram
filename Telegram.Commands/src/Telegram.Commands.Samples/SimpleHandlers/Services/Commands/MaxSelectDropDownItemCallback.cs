using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "selectdropdownitem_callback", Authorized = true, Swarms = new[] { "max" })]
public sealed class MaxSelectDropDownItemCallback(ISampleMessageSender sender)
    : SelectDropDownItemCallback(sender), ISessionCommand<global::Max.Bot.Types.CallbackQuery, DropDownSessionObject>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Max.Bot.Types.CallbackQuery, DropDownSessionObject>.Execute(QueryContext<global::Max.Bot.Types.CallbackQuery> query, DropDownSessionObject data)
        => Execute(query, data);
}
