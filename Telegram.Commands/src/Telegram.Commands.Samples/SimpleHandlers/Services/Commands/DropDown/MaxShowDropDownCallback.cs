using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "showdropdown_callback", Authorized = true, Platform = "max")]
public sealed class MaxShowDropDownCallback(ISampleMessageSender sender)
    : ShowDropDownCallback(sender), ISessionCommand<global::Max.Bot.Types.CallbackQuery, DropDownSessionObject>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Max.Bot.Types.CallbackQuery, DropDownSessionObject>.Execute(IQueryContext<global::Max.Bot.Types.CallbackQuery> query, DropDownSessionObject data)
        => Execute(query, data);
}
